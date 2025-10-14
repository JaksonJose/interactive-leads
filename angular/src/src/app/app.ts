import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { PRIME_NG_MODULES } from '@shared/primeng-imports';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ...PRIME_NG_MODULES],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private translate = inject(TranslateService);
  
  constructor() {
    this.translate.addLangs(['en-US', 'pt-BR']);
    this.translate.setFallbackLang('pt-BR');
    this.translate.use('pt-BR');
  }
}
