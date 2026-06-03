import { Routes } from '@angular/router';
import { InventoryComponent } from './components/inventory/inventory.component';
import { RecommendationsComponent } from './components/recommendations/recommendations.component';
import { ChatComponent } from './components/chat/chat.component';
import { OrderComponent } from './components/order/order.component';

export const routes: Routes = [
  { path: '', redirectTo: 'chat', pathMatch: 'full' },
  { path: 'chat', component: ChatComponent },
  { path: 'inventory', component: InventoryComponent },
  { path: 'recommendations', component: RecommendationsComponent },
  { path: 'order', component: OrderComponent }
];
