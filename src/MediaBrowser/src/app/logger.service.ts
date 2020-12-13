import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoggerService {

  constructor() { }

  public debug(msg : any) : void {
    this.log(LogLevel.Debug, msg);
  }

  public error(msg : any) : void {
    this.log(LogLevel.Error, msg);
  }

  public fatal(msg : any) : void {
    this.log(LogLevel.Fatal, msg);
  }

  public getLevelAsString(level : LogLevel) : string {
    switch (level) {
      case LogLevel.Debug:
        return 'DEBUG';
      case LogLevel.Info:
        return 'INFO';
      case LogLevel.Warn:
        return 'WARN';
      case LogLevel.Error:
        return 'ERROR';
      case LogLevel.Fatal:
        return 'FATAL';
    }
  }

  public getLevelColor(level : LogLevel) : string {
    switch (level) {
      case LogLevel.Debug:
        return 'teal;';
      case LogLevel.Info:
        return 'blue;';
      case LogLevel.Warn:
        return 'olive;';
      case LogLevel.Error:
        return 'red;';
      case LogLevel.Fatal:
        return 'red; font-weight: bold;';
    }
  }

  public info(msg : any) : void {
    this.log(LogLevel.Info, msg);
  }
  
  public log(level : LogLevel, msg : any) {
    if (this.minLevel <= level) {
      let now = new Date();
      console.log(`%c ${now.getFullYear()}-${(now.getMonth() + 1 + '').padStart(2, '0')}-${(now.getDate() + '').padStart(2, '0')} ${(now.getHours() + '').padStart(2, '0')}:${(now.getMinutes() + '').padStart(2, '0')}:${(now.getSeconds() + '').padStart(2, '0')}.${(now.getMilliseconds() + '').padStart(3, '0')} - ${this.getLevelAsString(level).padStart(5, ' ')} - ${typeof msg === 'string' ? msg : JSON.stringify(msg)}`, `background: #FFFFFF; color: ${this.getLevelColor(level)}`);
    }
  }

  public minLevel : LogLevel = LogLevel.Debug;

  public warn(msg : any) : void {
    this.log(LogLevel.Warn, msg);
  }
}

export enum LogLevel {
  Debug = 1,
  Info = 2,
  Warn = 3,
  Error = 4,
  Fatal = 5
}
