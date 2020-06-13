import { NgModule } from "@angular/core";
import { DashboardRoutingModule } from "./dashboard-routing.module";
import { DashboardComponent } from "./dashboard.component";
import { NgZorroAntdModule } from "ng-zorro-antd";
import { NzIconModule } from "ng-zorro-antd/icon";
import { IconDefinition } from "@ant-design/icons-angular";
import * as AllIcons from "@ant-design/icons-angular/icons";
import { FinancialComponent } from "./pages/financial/financial.component";
import { HighchartsChartModule } from 'highcharts-angular';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AssetTransitionComponent } from './pages/financial/asset/asset-transition/asset-transition.component';
import { UpdateRequestComponent } from './pages/financial/update-request/update-request.component';
import { DateRangeSelectorComponent } from './pages/financial/date-range-selector/date-range-selector.component';
import { AssetRatioComponent } from './pages/financial/asset/asset-ratio/asset-ratio.component';

const antDesignIcons = AllIcons as {
  [key: string]: IconDefinition;
};
const icons: IconDefinition[] = Object.keys(antDesignIcons).map(
  (key) => antDesignIcons[key]
);
@NgModule({
  declarations: [
    DashboardComponent,
    FinancialComponent,
    AssetTransitionComponent,
    UpdateRequestComponent,
    AssetRatioComponent,
    DateRangeSelectorComponent
  ],
  imports: [
    FormsModule,
    DashboardRoutingModule,
    NgZorroAntdModule,
    HighchartsChartModule,
    NzIconModule.forRoot(icons),
    CommonModule
  ]
})
export class DashboardModule { }
