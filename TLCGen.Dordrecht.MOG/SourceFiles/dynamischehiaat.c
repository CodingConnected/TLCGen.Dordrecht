/*
     Vijf zinnen over GOM en IVER'18 met verwijzing naar bronnen (Verkeerskunde voor GOM, G-C rapport / IVER website voor IVER'18)
*/

  /* Onderstaande functie 'hiaattijden_verlenging()' gaat er van uit dat de functie 'meetkriterium_prm_va_arg()' of
     'meetkriterium2_prm_va_arg()' (als er niet opgedrempeld mag worden) wordt gebruikt voor het normaal bepalen van het meetkriterium
	 De argumenten met een enkel streepje zijn de originele argumenten uit de IVER'18 voorbeeldcode van Willem Kinzel.
	 De argumenten met een dubbel streepje zijn toegevoegde argumenten tbv Dordrecht, aangepast door Dick den Ouden. In de code zijn de wijzigingen
	 aangegeven met / *-* / bij de functie of bij de aanpassing.
	 -- 1e argument: boolean tbv detectie vrijkomen koplus (tellers starten op ED[koplus] ipv op SG[fc])
     -  2e argument: array-nummer signaalgroep
     Dan per detectielus de volgende argumenten:
     -  rijstrook 1, 2, 3 of 4. Deze waarde wordt gebruikt voor het al dan niet opdrempelen. Als er wel opgedrempeld mag worden, dan altijd '1' invullen.
     -  array-nummer detectie-element
     -  array-nummer tijdelement - moment 1
     -  array-nummer tijdelement - moment 2
     -  array-nummer tijdelement - hiaattijd 1
     -  array-nummer tijdelement - hiaattijd 2
     -  array-nummer tijdelement - maximum groentijd
     -  springvoorwaarde  (op start groen, als er geen hiaatmeting meer is op deze en de stroomafwaartse lussen, meteen naar de 2e/lagere hiaattijd overgaan)
     -  verlengvoorwaarde (op start groen, als er geen hiaatmeting meer is op deze en de stroomafwaartse lussen, de verlengfunctie uitschakelen             )
	 -- extra verlengvoorwaarde (booleaanse conditie: bij TRUE altijd verlengen op deze lus; bijvoorbeeld bij aanwezigheid deelconflict (G[fc11] && G[fc36]))
	 -- array-nummer veiligheidsgroen rijtimer 1 (t.b.v. CORRECTIE op veiligheidsgroen uit stdfunc; de functie uit stdfunc dus OOK inschakelen              )
	 -- array-nummer veiligheidsgroen rijtimer 2
	 -- array-nummer memory element t.b.v. bewaren oorspronkelijke (statische) hiaattijd


        / * vrijkomkoplus, fc    * /
  hiaattijden_verlenging(1, fc11,                                                  
	                 / *  rijstr,  det,  moment1,  moment2,         tdh1,         tdh2,    maxtijd, s, v,        extra verl.vw,  tvag4_mvt_1,  tvag4_mvt_2,  stat.tdh, * /
	                           1, d111, td1101_1, td1101_2, ttdh_d1101_1, ttdh_d1101_2, tmax_d1101, 1, 1,                    0,           NG,           NG, meTDHd111,  
                               1, d113, td1103_1, td1103_2, ttdh_d1103_1, ttdh_d1103_2, tmax_d1103, 1, 1, (G[fc11] && G[fc36]),           NG,           NG, meTDHd113,  
                               1, d115, td1105_1, td1105_2, ttdh_d1105_1, ttdh_d1105_2, tmax_d1105, 1, 1,                    0,           NG,           NG, meTDHd115,  
                               1, d117, td1107_1, td1107_2, ttdh_d1107_1, ttdh_d1107_2, tmax_d1107, 1, 1,                    0, tvag4_11_1_1, tvag4_11_1_2, meTDHd117,  
																										          	   									    
                               2, d112, td1102_1, td1102_2, ttdh_d1102_1, ttdh_d1102_2, tmax_d1102, 1, 1,                    0,           NG,           NG, meTDHd112,  
                               2, d114, td1104_1, td1104_2, ttdh_d1104_1, ttdh_d1104_2, tmax_d1104, 1, 1, (G[fc11] && G[fc36]),           NG,           NG, meTDHd114,  
                               2, d116, td1106_1, td1106_2, ttdh_d1106_1, ttdh_d1106_2, tmax_d1106, 1, 1,                    0,           NG,           NG, meTDHd116,  
							   2, d118, td1108_1, td1108_2, ttdh_d1108_1, ttdh_d1108_2, tmax_d1108, 1, 1,                    0, tvag4_11_2_1, tvag4_11_2_2, meTDHd118,  
							   
							   END);




   */



void hiaattijden_verlenging(bool nietToepassen, bool vrijkomkop, count fc, ...)
{
  va_list argpt;                                    /* variabele argumentenlijst                                 */
  count dpnr;                                       /* arraynummer detectie-element                              */
  count t1, t2, tdh1, tdh2, tmax, tvag4_1, tvag4_2; /* arraynummers tijdelementen                                */
  count sthiaat;                                    /* arraynummer MM tbv bewaren statische hiaattijd            */
  count prmtdh;/*------------------------------------------------------------------------------------------------*/ /* nog verwijderen */
  count rijstrook_old = -1;                         /* vorige rijstrooknummer                                    */
  count rijstrook;                                  /* rijstrooknummer                                           */
  count max_rijstrook = 1;                          /* hoogste rijstrooknummer                                   */
  bool svw, vvw, evlvw, hulp_bit3, verlengen[5], tdh_saw[5];
  count dp_teller=0;                                /* telt aantal lussen vanaf stopstreep op bepaalde rijstrook */

  if (nietToepassen) return;

  /* initialisatie */
  for (rijstrook=0; rijstrook<5; rijstrook++)
  {
    verlengen[rijstrook] = FALSE;
    tdh_saw[rijstrook] = FALSE;                     /* TDH stroomafwaarts                                        */
    eavl[fc][rijstrook] = 0;                        /* eerste aktieve verlenglus (1 = eerste lus)                */
  }

  if (TRG[fc])  detstor[fc] = FALSE;                /* reset aanwezigheid detectiestoring voor deze fc bij TRG[] */ /*-*/

  /* vaststellen detectiestoring, alleen tijdens RV[] */ /*-*/
  if (RV[fc] && !TRG[fc] && !ERV[fc]) {
    va_start(argpt, fc);                            /* start var. argumentenlijst                                */
    do {
      rijstrook = va_arg(argpt, va_count);          /* lees rijstrooknummer                                      */
      if (rijstrook>=0) {																			            
        dpnr    = va_arg(argpt, va_count);          /* lees array-nummer detectie                                */
        t1      = va_arg(argpt, va_count);          /* ongebruikt                                                */
        t2      = va_arg(argpt, va_count);          /* ongebruikt                                                */
        tdh1    = va_arg(argpt, va_count);          /* ongebruikt                                                */
        tdh2    = va_arg(argpt, va_count);          /* ongebruikt                                                */
        tmax    = va_arg(argpt, va_count);          /* ongebruikt                                                */
        svw     = va_arg(argpt, va_bool);           /* ongebruikt                                                */
        vvw     = va_arg(argpt, va_bool);           /* ongebruikt                                                */
        evlvw   = va_arg(argpt, va_bool);           /* ongebruikt                                                */ /*-*/
        tvag4_1 = va_arg(argpt, va_count);          /* ongebruikt                                                */ /*-*/
        tvag4_2 = va_arg(argpt, va_count);          /* ongebruikt                                                */ /*-*/
        sthiaat = va_arg(argpt, va_count);          /* ongebruikt                                                */ /*-*/
		prmtdh  = va_arg(argpt, va_count);/*------------------------------------------------------------------------------------------------*/ /* nog verwijderen */
    
   
        #if defined (DL_type) && !defined (NO_DDFLUTTER) /* CCOL7 of hoger */
          if (CIF_IS[dpnr] >= CIF_DET_STORING /*|| OG[dpnr]*/ || BG[dpnr] || FL[dpnr])   detstor[fc] |= TRUE;
        #else
          if (CIF_IS[dpnr] >= CIF_DET_STORING /*|| OG[dpnr]*/ || BG[dpnr])               detstor[fc] |= TRUE;
        #endif
	    
        #ifndef AUTOMAAT  /* nog verwijderen */
          xyprintf(72,20+8,"detstor");
          xyprintf(72,20+8+rijstrook, "     %d",detstor[fc]);
        #endif
      }
    } while (rijstrook>=0);
    va_end(argpt);                     /* maak var. arg-lijst leeg */
  }

  va_start(argpt, fc);                              /* start var. argumentenlijst                                */
  do {
    rijstrook = va_arg(argpt, va_count);            /* lees rijstrooknummer                                      */
    if (rijstrook>=0 && (detstor[fc] != TRUE)) {																				            
      dpnr    = va_arg(argpt, va_count);            /* lees array-nummer detectie                                */
      t1      = va_arg(argpt, va_count);            /* lees array-nummer tijdelement - moment 1                  */
      t2      = va_arg(argpt, va_count);            /* lees array-nummer tijdelement - moment 2                  */
      tdh1    = va_arg(argpt, va_count);            /* lees array-nummer tijdelement - hiaattijd 1               */
      tdh2    = va_arg(argpt, va_count);            /* lees array-nummer tijdelement - hiaattijd 2               */
      tmax    = va_arg(argpt, va_count);            /* lees array-nummer tijdelement - maximum groentijd         */
      svw     = va_arg(argpt, va_bool);             /* lees springvoorwaarde  (op start groen, als er geen hiaatmeting meer is op deze en de stroomafwaartse lussen, meteen naar de 2e/lagere hiaattijd overgaan) */
      vvw     = va_arg(argpt, va_bool);             /* lees verlengvoorwaarde (op start groen, als er geen hiaatmeting meer is op deze en de stroomafwaartse lussen, de verlengfunctie uitschakelen             ) */
	  evlvw   = va_arg(argpt, va_bool);             /* lees extra verlengvoorwaarde (bijvoorbeeld tijdens aanwezig deelconflict      ) */ /*-*/
	  tvag4_1 = va_arg(argpt, va_count);            /* lees veiligheidsgroen 1e timer                            */ /*-*/
	  tvag4_2 = va_arg(argpt, va_count);            /* lees veiligheidsgroen 2e timer                            */ /*-*/
	  sthiaat = va_arg(argpt, va_count);            /* lees array-nummer MM tbv opslaan statisch hiaat           */ /*-*/
	  prmtdh  = va_arg(argpt, va_count);/*------------------------------------------------------------------------------------------------*/ /* nog verwijderen */


      /* tijdens R[] statische hiaten gebruiken tbv default TLCGen detectieopvang; det.storing resetten op TRG[] */ /*-*/
      if (SRA[fc]) {
          MM[sthiaat] = TDH_max[dpnr];              /* bewaar oorspronkelijke statische hiaattijd in MM          */
      }											    
      if (SGL[fc]) {							    
          TDH_max[dpnr] = MM[sthiaat];              /* zet oorspronkelijke statische hiaattijden terug           */
      }

      max_rijstrook = rijstrook;                    /* onthoud hoogste rijstrooknummer                           */
      if (rijstrook != rijstrook_old) {
        eavl[fc][rijstrook] = 0;
        dp_teller = 0;
      }
      dp_teller++;
      rijstrook_old = rijstrook;
	  
	  if (T_max[tmax]==0)  T_max[tmax] = (TFG_max[fc]+TVG_max[fc]); /* overnemen max groentijd                   */ /*-*/
	  
      /* actuele hiaattijd bepalen */
      RT[t1]   =
      RT[t2]   =
      RT[tmax] = SG[fc] || (vrijkomkop && (eavl[fc][rijstrook]==1)); /*-*/
	  
//      if (!G[fc]          )  PRM[prmtdh] = T_max[tdh1]; /*-*/
	  if (RA[fc] && !SRA[fc])  TDH_max[dpnr] = T_max[tdh1]; /*-*/
      if ( G[fc] &&   ET[t2])  TDH_max[dpnr] = T_max[tdh2];
      if ( G[fc] &&  !RT[t1] && !T[t1] && T[t2]) {
        /* hiaattijd wijzigt tussen t1 en t2 lineair, van tdh1 naar tdh2 */
        /* ---y------ = -------------x----------- * -----------------richtingscoëfficiënt=a-------------- + ----b------ */
          TDH_max[dpnr] = (T_timer[t2] - T_max[t1]) * (T_max[tdh2] - T_max[tdh1]) / (T_max[t2] - T_max[t1]) + T_max[tdh1];
      }
	  
      /* Als er geen verkeer is op de lus en de lussen ervoor bij start groen, meteen naar de 2e hiaattijd springen */
      if (SG[fc] && !tdh_saw[rijstrook] && dp_teller>1 && svw) {
        RT[t1] =
        RT[t2] = FALSE;
        TDH_max[dpnr] = T_max[tdh2];
        if (vvw) {
          RT[tmax] = FALSE;
        }
      }
	  
      /* bepalen of er stroomafwaarts van een lus hiaattijden lopen */
      if (TDH[dpnr]) {
        tdh_saw[rijstrook] = TRUE;
      }
	  
      /* Er mag verlengd worden op deze lus tot de timer is afgelopen */
	  /* of wanneer extra verlengvoorwaarde evlvw aanwezig is */
      if ((RT[tmax] || T[tmax] || evlvw) && G[fc] && TDH[dpnr]) { /*-*/ /* evlvw toegevoegd */
        verlengen[rijstrook] = TRUE;
      }
	  
      /* Bepaal eerste actieve verlenglus, vanaf de stopstreep gerekend */
      if ((RT[tmax] || T[tmax])    &&     /* maximum tijd loopt nog voor deze detector                         */
          G[fc]                    &&     /* signaalgroep is groen                                             */
          TDH[dpnr]                &&     /* hiaattijd van de detector loopt                                   */        /*-*/ /* wel naar TDH kijken */
          eavl[fc][rijstrook] == 0   ) {  /* eerste actieve verlenglus is op deze rijstrook nog niet ingesteld */
        eavl[fc][rijstrook] = dp_teller;
      }

      #ifndef AUTOMAAT /* nog verwijderen */
	    PRM[prmtdh] = TDH_max[dpnr];  
	    xyprintf(1,20+8,   " dp     tdh   t1   t2   h1   h2  max  tmmax");
	    xyprintf(1,20+1+dpnr, "%s     %3d  %3d  %3d  %3d  %3d  %3d    %3d",D_code[dpnr],PRM[prmtdh],T_timer[t1],T_timer[t2],T_max[tdh1],T_max[tdh2],T_max[tmax],T_timer[tmax]);
		xyprintf(50,20+8,"rijstr   eavl   verl");
		xyprintf(50,20+8+rijstrook, "  %3d    %3d      %d",rijstrook,eavl[fc][rijstrook],verlengen[rijstrook]);
      #endif

      /* afhandeling veiligheidsgroen, opgenomen in hoofdfunctie */ /*-*/
      if ((tvag4_1 != NG) && (tvag4_2 != NG) && SD[dpnr] && CIF_IS[dpnr]<CIF_DET_STORING) {  
        if (!T[tvag4_1]) {                              /* rijtimer 1 loopt nog niet              */
          RT[tvag4_1] = TRUE;
        }
        else {                                          /* rijtimer 1 loopt al                    */
          if (!T[tvag4_2]) {                            /* rijtimer 2 loopt nog niet              */
            RT[tvag4_2] = TRUE;
          }
          else {                                        /* beide rijtimers lopen al               */
            if (T_timer[tvag4_1] > T_timer[tvag4_2]) {  /* rijtimer 1 loopt langer dan timer 2    */
              RT[tvag4_1] = TRUE;
            }
            else {                                      /* rijtimer 2 loopt langer dan timer 1    */
              RT[tvag4_2] = TRUE;
            }
          }
        }
      }
      else {
        RT[tvag4_1] = FALSE;
        RT[tvag4_2] = FALSE;
      }
      
      if (!(RT[tvag4_1] || T[tvag4_1]) ||
          !(RT[tvag4_2] || T[tvag4_2])   ) {            /* minder dan 2 voertuigen in dilemmazone */
        YM[fc] &= ~BIT2;                                /* reset veiligheidsgroen                 */
      }
	  /* einde afhandeling veiligheidsgroen */
    }
  } while (rijstrook>=0);
  va_end(argpt);                     /* maak var. arg-lijst leeg */

  hulp_bit3 = FALSE;
  for (rijstrook=1; rijstrook<=max_rijstrook; rijstrook++)   /* voor alle rijstroken van de betreffende signaalgroep */
  {
    if (verlengen[rijstrook]) {
      hulp_bit3 = TRUE;
    }
    else
    {
      switch (rijstrook) {
        case 1 :
          MK[fc] &= ~(BIT1|BIT2);    /* reset meetkriterium rijstrook 1 */
          break;
        case 2 :
          MK[fc] &= ~(BIT4|BIT5);    /* reset meetkriterium rijstrook 2 */
          break;
        case 3 :
          MK[fc] &= ~(BIT6|BIT7);    /* reset meetkriterium rijstrook 3 */
          break;
        case 4 :
          MK[fc] &= ~(BIT8|BIT9);    /* reset meetkriterium rijstrook 4 */
          break;
      }
    }
  }

  if (!hulp_bit3) {                  /* als er geen enkele strook is waarop verlengd mag worden, ook BIT3 resetten */
    MK[fc] &= ~BIT3;                 /* reset meetkriterium BIT 3 */
  }
}
