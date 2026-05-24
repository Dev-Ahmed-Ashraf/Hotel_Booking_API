import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Api } from '../generated/api';
import {
  apiReviewsGet$Json,
  apiReviewsHotelHotelIdGet$Json,
  apiReviewsIdDelete,
  apiReviewsIdGet$Json,
  apiReviewsIdPatch,
  apiReviewsPost$Json,
  apiReviewsUserUserIdGet$Json,
} from '../generated/functions';
import type { ApiReviewsGet$Json$Params } from '../generated/fn/reviews/api-reviews-get-json';
import type { CreateReviewDto } from '../generated/models/create-review-dto';
import type { ReviewDto } from '../generated/models/review-dto';
import type { ReviewDtoPagedList } from '../generated/models/review-dto-paged-list';
import type { UpdateReviewDto } from '../generated/models/update-review-dto';
import { unwrapApiResponse } from '../operators/unwrap-api-response.operator';

@Injectable({ providedIn: 'root' })
export class ReviewsApiFacade {
  private readonly api = inject(Api);
  private readonly http = inject(HttpClient);

  getReviews(params?: ApiReviewsGet$Json$Params): Observable<ReviewDtoPagedList> {
    return apiReviewsGet$Json(this.http, this.api.rootUrl, params).pipe(
      map((response) => response.body),
      unwrapApiResponse<ReviewDtoPagedList>()
    );
  }

  getReviewsByHotel(
    hotelId: number,
    pageNumber = 1,
    pageSize = 10
  ): Observable<ReviewDtoPagedList> {
    return apiReviewsHotelHotelIdGet$Json(this.http, this.api.rootUrl, {
      hotelId,
      pageNumber,
      pageSize,
    }).pipe(
      map((response) => response.body),
      unwrapApiResponse<ReviewDtoPagedList>()
    );
  }

  getReviewsByUser(
    userId: number,
    pageNumber = 1,
    pageSize = 10
  ): Observable<ReviewDtoPagedList> {
    return apiReviewsUserUserIdGet$Json(this.http, this.api.rootUrl, {
      userId,
      pageNumber,
      pageSize,
    }).pipe(
      map((response) => response.body),
      unwrapApiResponse<ReviewDtoPagedList>()
    );
  }

  getReviewById(id: number): Observable<ReviewDto> {
    return apiReviewsIdGet$Json(this.http, this.api.rootUrl, { id }).pipe(
      map((response) => response.body),
      unwrapApiResponse<ReviewDto>()
    );
  }

  createReview(userId: number, body: CreateReviewDto): Observable<ReviewDto> {
    return apiReviewsPost$Json(this.http, this.api.rootUrl, { userId, body }).pipe(
      map((response) => response.body),
      unwrapApiResponse<ReviewDto>()
    );
  }

  updateReview(id: number, body: UpdateReviewDto): Observable<void> {
    return apiReviewsIdPatch(this.http, this.api.rootUrl, { id, body }).pipe(map(() => undefined));
  }

  deleteReview(id: number, isSoft = true): Observable<void> {
    return apiReviewsIdDelete(this.http, this.api.rootUrl, { id, isSoft }).pipe(map(() => undefined));
  }
}
