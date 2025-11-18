using AutoMapper;
using HomeService.Application.Commands.Reviews;
using HomeService.Application.Common;
using HomeService.Application.Mappings;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Reviews;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<ReviewDto>>
{
    private readonly IRepository<HomeService.Domain.Entities.Review> _reviewRepository;
    private readonly IRepository<HomeService.Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IUnitOfWork _unitOfWork;
        // private readonly SentimentAnalysisService _sentimentAnalysisService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateReviewCommandHandler> _logger;

    public CreateReviewCommandHandler(
        IRepository<HomeService.Domain.Entities.Review> reviewRepository,
        IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        IRepository<ServiceProvider> providerRepository,
        IUnitOfWork unitOfWork,
        // SentimentAnalysisService sentimentAnalysisService,
        IMapper mapper,
        ILogger<CreateReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _bookingRepository = bookingRepository;
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
        // _sentimentAnalysisService = sentimentAnalysisService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate booking
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return Result.Failure<ReviewDto>("Booking not found");
            }

            if (booking.CustomerId != request.CustomerId)
            {
                return Result.Failure<ReviewDto>("Unauthorized to review this booking");
            }

            if (booking.Status != BookingStatus.Completed)
            {
                return Result.Failure<ReviewDto>("Can only review completed bookings");
            }

            if (!booking.ProviderId.HasValue)
            {
                return Result.Failure<ReviewDto>("No provider assigned to this booking");
            }

            // Check if review already exists
            var existingReviews = await _reviewRepository.FindAsync(
                r => r.BookingId == request.BookingId,
                cancellationToken);

            if (existingReviews.Any())
            {
                return Result.Failure<ReviewDto>("Review already exists for this booking");
            }

            // Validate rating
            if (request.Rating < 1 || request.Rating > 5)
            {
                return Result.Failure<ReviewDto>("Rating must be between 1 and 5");
            }

            // Perform AI sentiment analysis if comment is provided
            decimal? sentimentScore = null;
            // DISABLED: SentimentAnalysisService not implemented in clean architecture
            // if (!string.IsNullOrWhiteSpace(request.Comment))
            // {
            //     try
            //     {
            //         var sentimentResult = await _sentimentAnalysisService.AnalyzeSentimentAsync(
            //             request.Comment,
            //             cancellationToken);
            //         sentimentScore = sentimentResult.Score;
            //     }
            //     catch (Exception ex)
            //     {
            //         _logger.LogWarning(ex, "Failed to analyze sentiment, continuing without it");
            //     }
            // }

            // Create review
            var review = new Domain.Entities.Review
            {
                Id = Guid.NewGuid(),
                BookingId = request.BookingId,
                CustomerId = request.CustomerId,
                ProviderId = booking.ProviderId.Value,
                Rating = request.Rating,
                Comment = request.Comment,
                ImageUrls = request.ImageUrls,
                VideoUrls = request.VideoUrls,
                SentimentScore = sentimentScore,
                IsVerified = true,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review, cancellationToken);

            // Update provider rating
            var provider = await _providerRepository.GetByIdAsync(booking.ProviderId.Value, cancellationToken);
            if (provider != null)
            {
                provider.TotalReviews++;
                provider.AverageRating = ((provider.AverageRating * (provider.TotalReviews - 1)) + request.Rating) / provider.TotalReviews;
                await _providerRepository.UpdateAsync(provider, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reviewDto = _mapper.Map<ReviewDto>(review);

            _logger.LogInformation("Review created for booking {BookingId} with rating {Rating}", request.BookingId, request.Rating);

            return Result.Success(reviewDto, "Review created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            return Result.Failure<ReviewDto>("An error occurred while creating review", ex.Message);
        }
    }
}
