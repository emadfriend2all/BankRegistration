import { Directive, HostListener, ElementRef, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Directive({
  selector: '[appIdMask]',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => IdMaskDirective),
      multi: true
    }
  ]
})
export class IdMaskDirective implements ControlValueAccessor {
  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};
  private rawValue = '';

  constructor(private el: ElementRef) {}

  @HostListener('input', ['$event'])
  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, ''); // Remove all non-digits
    
    // Limit to 11 digits
    if (value.length > 11) {
      value = value.substring(0, 11);
    }
    
    // Apply mask ###-####-####
    this.rawValue = value; // Store clean value (digits only)
    const maskedValue = this.applyMask(value);
    
    // Update input display
    input.value = maskedValue;
    
    // Notify form control of clean value
    this.onChange(this.rawValue);
  }

  @HostListener('blur')
  onBlur(): void {
    this.onTouched();
  }

  private applyMask(value: string): string {
    if (!value) return '';
    
    // Apply ###-####-#### format
    if (value.length <= 3) {
      return value;
    } else if (value.length <= 7) {
      return `${value.slice(0, 3)}-${value.slice(3)}`;
    } else {
      return `${value.slice(0, 3)}-${value.slice(3, 7)}-${value.slice(7, 11)}`;
    }
  }

  // ControlValueAccessor implementation
  writeValue(value: string): void {
    this.rawValue = value || '';
    const maskedValue = this.applyMask(this.rawValue);
    this.el.nativeElement.value = maskedValue;
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.el.nativeElement.disabled = isDisabled;
  }
}
