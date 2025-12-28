import { Component } from '@angular/core';

@Component({
    standalone: true,
    selector: 'app-footer',
    template: `<div class="layout-footer">
        Powered By
        <a href="" target="_blank" rel="noopener noreferrer" class="text-primary font-bold hover:underline">Target Point</a>
    </div>`
})
export class AppFooter {}
