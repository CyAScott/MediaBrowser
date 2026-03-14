import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { MediaService } from '../../services';
import { PeopleData, PeopleSectionComponent } from './people-section.component';

describe('PeopleSectionComponent', () => {
  const emptyPeople: PeopleData = {
    cast: [],
    directors: [],
    genres: [],
    producers: [],
    writers: []
  };

  const loadedPeople: PeopleData = {
    cast: ['Cast One'],
    directors: ['Director One'],
    genres: ['Drama'],
    producers: ['Producer One'],
    writers: ['Writer One']
  };

  let mediaServiceMock: {
    getAllTags: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    localStorage.clear();
    PeopleSectionComponent.allPeople = {
      cast: [],
      directors: [],
      genres: [],
      producers: [],
      writers: []
    };

    mediaServiceMock = {
      getAllTags: vi.fn((tagType: keyof PeopleData) => of(loadedPeople[tagType]))
    };

    await TestBed.configureTestingModule({
      imports: [PeopleSectionComponent],
      providers: [{ provide: MediaService, useValue: mediaServiceMock }]
    }).compileComponents();
  });

  it('creates the component', () => {
    const fixture = TestBed.createComponent(PeopleSectionComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('loads people lists from cache when available', async () => {
    localStorage.setItem(PeopleSectionComponent.CACHE_KEY, JSON.stringify(loadedPeople));

    const fixture = TestBed.createComponent(PeopleSectionComponent);
    await fixture.componentInstance.ngOnInit();

    expect(PeopleSectionComponent.allPeople).toEqual(loadedPeople);
    expect(mediaServiceMock.getAllTags).not.toHaveBeenCalled();
  });

  it('fetches people lists and caches them when cache is empty', async () => {
    const setItemSpy = vi.spyOn(Storage.prototype, 'setItem');

    const fixture = TestBed.createComponent(PeopleSectionComponent);
    await fixture.componentInstance.ngOnInit();

    expect(mediaServiceMock.getAllTags).toHaveBeenCalledTimes(5);
    expect(mediaServiceMock.getAllTags).toHaveBeenNthCalledWith(1, 'cast');
    expect(mediaServiceMock.getAllTags).toHaveBeenNthCalledWith(2, 'directors');
    expect(mediaServiceMock.getAllTags).toHaveBeenNthCalledWith(3, 'genres');
    expect(mediaServiceMock.getAllTags).toHaveBeenNthCalledWith(4, 'producers');
    expect(mediaServiceMock.getAllTags).toHaveBeenNthCalledWith(5, 'writers');
    expect(PeopleSectionComponent.allPeople).toEqual(loadedPeople);
    expect(setItemSpy).toHaveBeenCalledWith(PeopleSectionComponent.CACHE_KEY, JSON.stringify(loadedPeople));

    setItemSpy.mockRestore();
  });

  it('adds and removes array items and emits changes', () => {
    const fixture = TestBed.createComponent(PeopleSectionComponent);
    const component = fixture.componentInstance;
    component.peopleData = structuredClone(emptyPeople);

    const emitSpy = vi.spyOn(component.peopleDataChange, 'emit');

    component.addArrayItem('cast');
    expect(component.peopleData.cast).toEqual(['']);
    expect(emitSpy).toHaveBeenCalledWith(component.peopleData);

    component.removeArrayItem('cast', 0);
    expect(component.peopleData.cast).toEqual([]);
    expect(emitSpy).toHaveBeenCalledTimes(2);
  });

  it('updates selected suggestion and emits people change', () => {
    const fixture = TestBed.createComponent(PeopleSectionComponent);
    const component = fixture.componentInstance;
    component.peopleData = {
      cast: ['old'],
      directors: [],
      genres: [],
      producers: [],
      writers: []
    };

    const emitSpy = vi.spyOn(component.peopleDataChange, 'emit');

    component.onSuggestionSelected('cast', 0, 'new value');

    expect(component.peopleData.cast[0]).toBe('new value');
    expect(emitSpy).toHaveBeenCalledWith(component.peopleData);
  });

  it('returns suggestions from static cache helpers', () => {
    PeopleSectionComponent.allPeople = structuredClone(loadedPeople);
    const fixture = TestBed.createComponent(PeopleSectionComponent);
    const component = fixture.componentInstance;

    expect(component.getCastSuggestions()).toEqual(loadedPeople.cast);
    expect(component.getDirectorsSuggestions()).toEqual(loadedPeople.directors);
    expect(component.getGenresSuggestions()).toEqual(loadedPeople.genres);
    expect(component.getProducersSuggestions()).toEqual(loadedPeople.producers);
    expect(component.getWritersSuggestions()).toEqual(loadedPeople.writers);
    expect(component.trackByIndex(2)).toBe(2);
  });

  it('clears stale cache when local data has unknown values', () => {
    PeopleSectionComponent.allPeople = structuredClone(loadedPeople);
    const removeSpy = vi.spyOn(Storage.prototype, 'removeItem');

    PeopleSectionComponent.clearCacheIfStale({
      cast: ['Unknown Cast'],
      directors: [],
      genres: [],
      producers: [],
      writers: []
    });

    expect(removeSpy).toHaveBeenCalledWith(PeopleSectionComponent.CACHE_KEY);
    removeSpy.mockRestore();
  });

  it('does not clear cache when local values are all known', () => {
    PeopleSectionComponent.allPeople = structuredClone(loadedPeople);
    const removeSpy = vi.spyOn(Storage.prototype, 'removeItem');

    PeopleSectionComponent.clearCacheIfStale(structuredClone(loadedPeople));

    expect(removeSpy).not.toHaveBeenCalled();
    removeSpy.mockRestore();
  });
});
