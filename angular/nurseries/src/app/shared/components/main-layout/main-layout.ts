import { Component } from '@angular/core';
import { Navbar } from "../navbar/navbar";
import { Footer } from "../footer/footer";
import { RouterModule } from "@angular/router";

@Component({
  selector: 'app-main-layout',
  imports: [Navbar, Footer, RouterModule],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css',
})
export class MainLayout {}
