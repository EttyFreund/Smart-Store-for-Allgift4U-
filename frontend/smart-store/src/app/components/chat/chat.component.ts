import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InventoryOrderService } from '../../services/inventory-order.service';
import { RecommendationService } from '../../services/recommendation.service';

interface ChatMessage {
  role: 'user' | 'bot';
  text: string;
  items?: { productName: string; quantityRemoved: number; newQuantity: number; aiTriggered: boolean }[];
  recommendations?: { supplierName: string; price: number; purchaseUrl?: string }[];
}

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss'
})
export class ChatComponent {
  messages: ChatMessage[] = [
    { role: 'bot', text: 'שלום! אני מערכת ניהול המלאי של All Gift 🎁\nאיזו הזמנה תרצי לעבד היום?' }
  ];
  input = '';
  loading = false;

  constructor(
    private inventoryService: InventoryOrderService,
    private recService: RecommendationService,
    private cd: ChangeDetectorRef
  ) {}

  send(): void {
    const text = this.input.trim();
    if (!text || this.loading) return;

    this.messages.push({ role: 'user', text });
    this.input = '';
    this.loading = true;

    this.inventoryService.processOrder(text).subscribe({
      next: (res: any) => {
        const name = res.orderName ?? text;
        const items = res.items ?? [];
        const aiTriggered = items.some((i: any) => i.aiTriggered);

        if (aiTriggered) {
          this.recService.getRecommendations().subscribe({
            next: (recs) => {
              this.messages.push({
                role: 'bot',
                text: `✅ ההזמנה "${name}" עובדה בהצלחה`,
                items,
                recommendations: recs.slice(-items.filter((i: any) => i.aiTriggered).length)
              });
              this.loading = false;
              this.cd.detectChanges();
            },
            error: () => {
              this.messages.push({ role: 'bot', text: `✅ ההזמנה "${name}" עובדה בהצלחה`, items });
              this.loading = false;
              this.cd.detectChanges();
            }
          });
        } else {
          this.messages.push({ role: 'bot', text: `✅ ההזמנה "${name}" עובדה בהצלחה`, items });
          this.loading = false;
          this.cd.detectChanges();
        }
      },
      error: (err) => {
        this.messages.push({ role: 'bot', text: `❌ ${err.error?.error ?? 'שגיאה בעיבוד ההזמנה'}` });
        this.loading = false;
        this.cd.detectChanges();
      }
    });
  }

  onEnter(event: KeyboardEvent): void {
    if (event.key === 'Enter') this.send();
  }
}
