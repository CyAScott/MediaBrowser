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
    getAllCast: ReturnType<typeof vi.fn>;
    getAllDirectors: ReturnType<typeof vi.fn>;
    getAllGenres: ReturnType<typeof vi.fn>;
    getAllProducers: ReturnType<typeof vi.fn>;
    getAllWriters: ReturnType<typeof vi.fn>;
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
      getAllCast: vi.fn().mockReturnValue(of(loadedPeople.cast)),
      getAllDirectors: vi.fn().mockReturnValue(of(loadedPeople.directors)),
      getAllGenres: vi.fn().mockReturnValue(of(loadedPeople.genres)),
      getAllProducers: vi.fn().mockReturnValue(of(loadedPeople.producers)),
      getAllWriters: vi.fn().mockReturnValue(of(loadedPeople.writers))
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
    expect(mediaServiceMock.getAllCast).not.toHaveBeenCalled();
    expect(mediaServiceMock.getAllDirectors).not.toHaveBeenCalled();
    expect(mediaServiceMock.getAllGenres).not.toHaveBeenCalled();
    expect(mediaServiceMock.getAllProducers).not.toHaveBeenCalled();
    expect(mediaServiceMock.getAllWriters).not.toHaveBeenCalled();
  });

  it('fetches people lists and caches them when cache is empty', async () => {
    const setItemSpy = vi.spyOn(Storage.prototype, 'setItem');

    const fixture = TestBed.createComponent(PeopleSectionComponent);
    await fixture.componentInstance.ngOnInit();

    expect(mediaServiceMock.getAllCast).toHaveBeenCalledTimes(1);
    expect(mediaServiceMock.getAllDirectors).toHaveBeenCalledTimes(1);
    expect(mediaServiceMock.getAllGenres).toHaveBeenCalledTimes(1);
    expect(mediaServiceMock.getAllProducers).toHaveBeenCalledTimes(1);
    expect(mediaServiceMock.getAllWriters).toHaveBeenCalledTimes(1);
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
