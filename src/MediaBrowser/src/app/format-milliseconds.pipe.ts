import { Pipe, PipeTransform } from '@angular/core';
import * as moment from 'moment';

@Pipe({
  name: 'formatMilliseconds'
})
export class FormatMillisecondsPipe implements PipeTransform {

  transform(value : number, format : string | undefined) : string {
    return value ? moment.utc(moment.duration(value).as('milliseconds')).format(format || 'H:mm:ss') : '';
  }

}
