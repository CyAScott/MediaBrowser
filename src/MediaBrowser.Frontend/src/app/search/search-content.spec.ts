import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { MediaReadModel } from '../services/media.service';
import { SearchContentComponent } from './search-content';

function createMediaResult(overrides: Partial<MediaReadModel> = {}): MediaReadModel {
  return {
    id: 'media-1',
    path: '/tmp/media-1.mp4',
    title: 'Media One',
    originalTitle: 'Media One',
    description: 'Desc',
    mime: 'video/mp4',
    md5: 'abc123',
    published: '2024-01-01',
    ctimeMs: '0',
    mtimeMs: '0',
    createdOn: new Date('2024-01-01T00:00:00.000Z'),
    updatedOn: new Date('2024-01-02T00:00:00.000Z'),
    ffprobe: {},
    cast: [],
    directors: [],
    genres: [],
    producers: [],
    writers: [],
    url: 'https://example.com/media-1.mp4',
    ...overrides
  };
}

describe('SearchContentComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SearchContentComponent]
    }).compileComponents();
  });

  it('creates the component', () => {
    const fixture = TestBed.createComponent(SearchContentComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('emits card click and scroll events', () => {
    const fixture = TestBed.createComponent(SearchContentComponent);
    const component = fixture.componentInstance;

    const cardClickSpy = vi.spyOn(component.cardClick, 'emit');
    const scrollSpy = vi.spyOn(component.scroll, 'emit');
    const event = new Event('scroll');

    component.onCardClick();
    component.onScroll(event);

    expect(cardClickSpy).toHaveBeenCalledTimes(1);
    expect(scrollSpy).toHaveBeenCalledWith(event);
  });

  it('builds tooltip with optional media metadata', () => {
    const fixture = TestBed.createComponent(SearchContentComponent);
    const component = fixture.componentInstance;

    const tooltip = component.getTooltip(createMediaResult({
      duration: 3661,
      userStarRating: 4,
      cast: ['Cast One'],
      genres: ['Action'],
      directors: ['Director One'],
      producers: ['Producer One'],
      writers: ['Writer One']
    }));

    expect(tooltip).toContain('Media One');
    expect(tooltip).toContain('Duration:');
    expect(tooltip).toContain('Rating: 4 star(s)');
    expect(tooltip).toContain('Cast: Cast One');
    expect(tooltip).toContain('Genres: Action');
    expect(tooltip).toContain('Directors: Director One');
    expect(tooltip).toContain('Producers: Producer One');
    expect(tooltip).toContain('Writers: Writer One');
  });

  it('returns title-only tooltip when optional values are missing', () => {
    const fixture = TestBed.createComponent(SearchContentComponent);
    const component = fixture.componentInstance;

    const tooltip = component.getTooltip(createMediaResult({ title: 'Only Title' }));

    expect(tooltip).toBe('Only Title');
  });

  it('tracks result cards by id', () => {
    const fixture = TestBed.createComponent(SearchContentComponent);
    const component = fixture.componentInstance;

    expect(component.trackByResultId(0, createMediaResult({ id: 'custom-id' }))).toBe('custom-id');
  });
});
