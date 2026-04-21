import { Params } from "@angular/router";
import { SearchMediaRequest } from "../services";

export class SearchQueryParams {
  public cast: string[] = [];
  public descending: boolean = false;
  public directors: string[] = [];
  public genres: string[] = [];
  public keywords: string = '';
  public pageIndex: number = 0;
  public producers: string[] = [];
  public sort: 'title' | 'createdOn' | 'duration' | 'userStarRating' | 'random' = 'title';
  public writers: string[] = [];

  static readonly DEFAULT_SORT = 'title';

  public loadFromQueryParams(params: Params): void {
    // Load search parameters from URL
    this.keywords = params['keywords'] || '';
    this.sort = params['sort'] || SearchQueryParams.DEFAULT_SORT;
    this.descending = params['descending'] === 'true';
    
    // Handle array parameters
    if (params['cast']) {
      this.cast = Array.isArray(params['cast']) ? params['cast'] : [params['cast']];
    } else {
      this.cast = [];
    }
    
    if (params['directors']) {
      this.directors = Array.isArray(params['directors']) ? params['directors'] : [params['directors']];
    } else {
      this.directors = [];
    }
    
    if (params['genres']) {
      this.genres = Array.isArray(params['genres']) ? params['genres'] : [params['genres']];
    } else {
      this.genres = [];
    }

    this.pageIndex = params['pageIndex'] ? parseInt(params['pageIndex'], 10) : 0;

    if (params['producers']) {
      this.producers = Array.isArray(params['producers']) ? params['producers'] : [params['producers']];
    } else {
      this.producers = [];
    }

    if (params['writers']) {
      this.writers = Array.isArray(params['writers']) ? params['writers'] : [params['writers']];
    } else {
      this.writers = [];
    }
  }

  public getQueryParams(includePageIndex: boolean = false): any {
    const queryParams: any = {};
    
    // Only add parameters that have values to keep URL clean
    if (this.keywords?.trim()) {
      queryParams['keywords'] = this.keywords.trim();
    }
    if (this.cast.length > 0) {
      queryParams['cast'] = this.cast;
    }
    if (this.directors.length > 0) {
      queryParams['directors'] = this.directors;
    }
    if (this.genres.length > 0) {
      queryParams['genres'] = this.genres;
    }
    if (this.producers.length > 0) {
      queryParams['producers'] = this.producers;
    }
    if (this.writers.length > 0) {
      queryParams['writers'] = this.writers;
    }
    queryParams['sort'] = this.sort;
    if (this.descending) {
      queryParams['descending'] = 'true';
    }
    if (includePageIndex) {
      queryParams['pageIndex'] = this.pageIndex;
    }
    return queryParams;
  }

  public getSearchMediaRequest(skip?: number, take?: number): SearchMediaRequest {
    const request: SearchMediaRequest = {
      skip: skip,
      sort: this.sort,
      take: take,
    };

    if (this.keywords?.trim()) {
      request.keywords = this.keywords.trim();
    }
    if (this.cast.length > 0) {
      request.cast = [...this.cast];
    }
    if (this.directors.length > 0) {
      request.directors = [...this.directors];
    }
    if (this.genres.length > 0) {
      request.genres = [...this.genres];
    }
    if (this.descending) {
      request.descending = this.descending;
    }
    if (this.producers.length > 0) {
      request.producers = [...this.producers];
    }
    if (this.writers.length > 0) {
      request.writers = [...this.writers];
    }

    return request;
  }
}