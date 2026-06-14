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

  public static loadFromQueryParams(searchQueryParams: SearchQueryParams, params: Params, setPageIndex: boolean): void {
    // Load search parameters from URL
    searchQueryParams.keywords = params['keywords'] || '';
    searchQueryParams.sort = params['sort'] || SearchQueryParams.DEFAULT_SORT;
    searchQueryParams.descending = params['descending'] === 'true';
    
    // Handle array parameters
    if (params['cast']) {
      searchQueryParams.cast = Array.isArray(params['cast']) ? params['cast'] : [params['cast']];
    } else {
      searchQueryParams.cast = [];
    }
    
    if (params['directors']) {
      searchQueryParams.directors = Array.isArray(params['directors']) ? params['directors'] : [params['directors']];
    } else {
      searchQueryParams.directors = [];
    }
    
    if (params['genres']) {
      searchQueryParams.genres = Array.isArray(params['genres']) ? params['genres'] : [params['genres']];
    } else {
      searchQueryParams.genres = [];
    }

    if (setPageIndex) {
      searchQueryParams.pageIndex = params['pageIndex'] ? parseInt(params['pageIndex'], 10) : 0;
    }

    if (params['producers']) {
      searchQueryParams.producers = Array.isArray(params['producers']) ? params['producers'] : [params['producers']];
    } else {
      searchQueryParams.producers = [];
    }

    if (params['writers']) {
      searchQueryParams.writers = Array.isArray(params['writers']) ? params['writers'] : [params['writers']];
    } else {
      searchQueryParams.writers = [];
    }
  }

  public static getQueryParams(searchQueryParams: SearchQueryParams, includePageIndex: boolean): any {
    const queryParams: any = {};
    
    // Only add parameters that have values to keep URL clean
    if (searchQueryParams.keywords?.trim()) {
      queryParams['keywords'] = searchQueryParams.keywords.trim();
    }
    if (searchQueryParams.cast.length > 0) {
      queryParams['cast'] = searchQueryParams.cast;
    }
    if (searchQueryParams.directors.length > 0) {
      queryParams['directors'] = searchQueryParams.directors;
    }
    if (searchQueryParams.genres.length > 0) {
      queryParams['genres'] = searchQueryParams.genres;
    }
    if (searchQueryParams.producers.length > 0) {
      queryParams['producers'] = searchQueryParams.producers;
    }
    if (searchQueryParams.writers.length > 0) {
      queryParams['writers'] = searchQueryParams.writers;
    }
    queryParams['sort'] = searchQueryParams.sort;
    if (searchQueryParams.descending) {
      queryParams['descending'] = 'true';
    }
    if (includePageIndex) {
      queryParams['pageIndex'] = searchQueryParams.pageIndex;
    }
    return queryParams;
  }

  public static getSearchMediaRequest(searchQueryParams: SearchQueryParams, skip?: number, take?: number): SearchMediaRequest {
    const request: SearchMediaRequest = {
      skip: skip,
      sort: searchQueryParams.sort,
      take: take,
    };

    if (searchQueryParams.keywords?.trim()) {
      request.keywords = searchQueryParams.keywords.trim();
    }
    if (searchQueryParams.cast.length > 0) {
      request.cast = [...searchQueryParams.cast];
    }
    if (searchQueryParams.directors.length > 0) {
      request.directors = [...searchQueryParams.directors];
    }
    if (searchQueryParams.genres.length > 0) {
      request.genres = [...searchQueryParams.genres];
    }
    if (searchQueryParams.descending) {
      request.descending = searchQueryParams.descending;
    }
    if (searchQueryParams.producers.length > 0) {
      request.producers = [...searchQueryParams.producers];
    }
    if (searchQueryParams.writers.length > 0) {
      request.writers = [...searchQueryParams.writers];
    }

    return request;
  }
}