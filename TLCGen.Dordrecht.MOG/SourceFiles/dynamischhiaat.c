
/*
   BESTAND:   dynamischhiaat.c
   TLCGEN :   0.3.7 en hoger

   ****************************** Versie commentaar ***********************************
   *
   * Versie   Datum        Ontwerper   Commentaar
   * 1.0.0    25- 1-2018   Kzw         Basisversie i.o.v. IVER
   * 2.0.0    05-12-2018   ddo         Diverse aanpassingen voor stedelijk gebruik
   * 2.1.0    17-12-2018   ddo         Diverse aanpassingen na Vissim simulatie
   *
   ************************************************************************************

   Zowel bij de IVER detectieconfiguratie uit 2018 (IVER'18) als bij Groen Op Maat (GOM) wordt gebruik
   gemaakt van dynamische hiaattijden. Met onderstaande code kunnen beide detectieconfiguraties worden bediend.

   Dynamische hiaattijden zijn bedoeld om op efficiënte wijze groen te verlengen, waarbij aan de 'voorkant'
   minder vastgroen en geen koplusmaximum* nodig zijn, en aan de 'achterkant' gebruik gemaakt kan worden
   van een deel van de geeltijd. Daarbij wordt gebruik gemaakt van een specifieke detectie configuratie.

   De methodiek is als GOM ontwikkeld door Luuk Misdom en IT&T (nu Vialis). Voor meer informatie, zie het Handboek
   Verleerslichtenregelingen 2014 p. 294, Verkeerskunde nummer 06-09, of de website van IT&T:
   - http://www.it-t.nl/wp-content/uploads/Vk06_09-00-art-Groen-op-maat-LR.pdf
   - http://www.it-t.nl/wp-content/uploads/Groen-op-Maat-configuraties.pdf

   In augustus 2018 heeft de IVER een (nieuwe) detectieconfiguratie gepubliceerd die ook gebruik maakt van
   dynamische hiattijden. Zie voor de rapportage "Onderzoek detectieconfiguratie en signaalgroepafhandeling"
   van Goudappel Coffeng (in opdracht van IVER):
   - https://www.crow.nl/thema-s/verkeersmanagement/iver onder 'Downloads'.

   De implementatie van dynamische hiaattijden voor de TLCGen en de effecten ervan zijn nog NIET getest met
   intergroen of met gekoppelde regelingen. De huidige implementatie is een eerste versie kan daarom nog
   onverwachte effecten geven; zorgvuldig testen wordt aanbevolen.

   --

   Onderstaande functie 'hiaattijden_verlenging()' gaat er van uit dat de functie 'meetkriterium_prm_va_arg()' of
   'meetkriterium2_prm_va_arg()' (als er niet opgedrempeld mag worden) wordt gebruikt voor het normaal bepalen van het meetkriterium.

   De argumenten met een enkel streepje zijn de originele argumenten uit de IVER'18 voorbeeldcode van Goudappel Coffeng (Willem Kinzel).
   De argumenten met een dubbel streepje zijn toegevoegde argumenten tbv het gemeentelijk wegennet (Dick den Ouden).
   In de code zijn de wijzigingen aangegeven met / *-* / bij de functie of bij de aanpassing.

   De code is vermoedelijk nog voor verbetering vatbaar; feedback wordt gewaardeerd en kan naar d.denouden@ll-t.nl

   Beschrijving van de aanroep:
   -- 1e argument: boolean tbv bepaling functie wel/niet gebruiken voor deze fase (bijvoorbeeld niet bij brugopening), uitgevoerd als hulpelement
				   Indien waarde == TRUE wordt de functie niet doorlopen.
   -- 2e argument: boolean tbv detectie vrijkomen koplus (tellers starten op ED[koplus] ipv op SG[fc]), uitgevoerd als schakelaar
				   Indien waarde == TRUE worden de timers gestart bij het voor het eerst na SG[] afvallen van de koplus.
   -- 3e argument: array-nummer memory element meetkriterium
   -  4e argument: array-nummer signaalgroep
   Dan per detectielus de volgende argumenten:
   -  rijstrook 1, 2, 3 of 4. Deze waarde wordt gebruikt voor het al dan niet opdrempelen. Als er wel opgedrempeld mag worden, dan via een schakelaar of
	  in de regeling (bijvoorbeeld tbv comfort in de dalperiode) het hulpelement H[hopdrempelen<fc>] hoog maken.
   -  array-nummer detectie-element
   -  array-nummer tijdelement - moment 1
   -  array-nummer tijdelement - moment 2
   -  array-nummer tijdelement - hiaattijd 1
   -  array-nummer tijdelement - hiaattijd 2
   -  array-nummer tijdelement - maximum groentijd (wanneer '0' wordt ingevuld, gebruikt de code de vigerende maximum groentijd TFG_max[] + TVG_max[]     )
   -- array-nummer parameter detectorvoorwaarden (zie hieronder)
   -- array-nummer hulpelement extra verlengvoorwaarde (zie hieronder)
   -- array-nummer veiligheidsgroen rijtimer 1 (t.b.v. CORRECTIE op veiligheidsgroen uit stdfunc; de functie uit stdfunc dus OOK inschakelen              )
   -- array-nummer veiligheidsgroen rijtimer 2
   -- array-nummer memory element t.b.v. bewaren oorspronkelijke (statische) hiaattijd

   De parameter 'detectorvoorwaarden' omvat de volgende opties:
   -  springvoorwaarde  (op start groen, als er geen hiaatmeting      is op         de stroomafwaartse lussen, meteen naar de 2e/lagere hiaattijd overgaan)
   -  verlengvoorwaarde (op start groen, als er geen hiaatmeting meer is op deze en de stroomafwaartse lussen, de verlengfunctie UITschakelen             )
   -- extra verlengvoorwaarde (bij TRUE altijd verlengen op deze lus; bijvoorbeeld bij permanente aanwezigheid deelconflict (G[fc11] && G[fc36])          )
   -- aftelvoorwaarde (tijdens groen, als er wél hiaatmeting is op deze lus maar niet op de stroomafwartse lussen, meteen TDH_max[] gaan aftellen         )
   -- spring-tijdens-groen voorwaarde (wanneer tijdens G[] het hiaat valt op een detector, wordt de volgende detector stroomopwaarts de aktieve verlenglus)
	  De spring-tijdens-groen voorwaarde is voornamelijk bedoeld voor een GOM detectieveld, waar de eerste en tweede lange lus dichter bij elkaar liggen
	  dan bij een IVER'18 detectieveld.

	  NB: de verlengvoorwaarde is dus eigenlijk een NIETverleng voorwaarde!

   Het hulpelement 'extra verlengvoorwaarde' dient om vanuit de regelapplicatie "incidentele" verlenging op de detector af te dwingen, bijvoorbeeld
   tijdens file, ontruiming van een spoorwegovergang, of bij een niet-permanent deelconflict.

   In de parameter 'detectorvoorwaarden' worden de springvoorwaarde, verlengvoorwaarde, extra verlengvoorwaarde, aftelvoorwaarde en spring-tijdens-groen
   voorwaarde bitsgewijs opgeslagen in één parameter; de instellingen zijn daardoor ook op straat te wijzigen:
   BIT    dec.   betekenis                          optie in TLCGen
   BIT0 =  1  -  springvoorwaarde                 - 'SpringStart'     <<==   AANPASSEN (was: 'Spring')
   BIT1 =  2  -  verlengvoorwaarde                - 'VerlengNiet'
   BIT2 =  4  -- extra verlengvoorwaarde          - 'VerlengExtra'    <<==   AANPASSEN (was: 'Verleng wel')
   BIT3 =  8  -- aftelvoorwaarde                  - 'DirectAftel'
   BIT4 = 16  -- spring-tijdens-groen voorwaarde  - 'SpringGroen'

   De waardes kunnen worden opgeteld, bijvoorbeeld verlengvoorwaarde én aftelvoorwaarde voor dezelfde detector maakt waarde 10.
   Het aktief worden extra verlengvoorwaarde kan tevens worden aangestuurd vanuit het regelprogramma via een hulpelement (IH[] = G[fc1] && G[fc2]).
   De spring-tijdens-groen voorwaarde (svwG, BIT4) is nog NIET instelbaar in de generator.

   Er wordt verlengd op de aktieve lus én op de lussen stroomopwaarts van de aktieve lus.

   De hiaattijden worden niet aangepast wanneer een detectiestoring is geconstateerd. In dat geval wordt de statische hiaattijd gebruikt en dient
   de gebruiker een eigen detectiestoringsopvang te programmeren, of die uit de TLCGen te gebruiken.

   Statische hiaattijden kunnen (alléén) tijdens (RV[] && !TRG[]) blijvend worden aangepast; daarbuiten worden de hiaattijden overschreven
   door de in onderstaande code berekende dynamische hiaattijd.


   Voorbeeld voor 2 rijstroken:
									1e argument,      2e argument, 3e arg, 4e arg,
   hiaattijden_verlenging ( IH[hgeendynhiaat05], SCH[schedkop_05],  mmk05,   fc05,

					 rijstrook,  det,  moment1,  moment2,       tdh1,       tdh2,  maxtijd,      spring- en verlengvoorwaarden, tvag4_mvt_1,  tvag4_mvt_2, stat.TDH,
	IH[hopdrempelen05] ? 1 : 1, d051,   t051_1,   t051_2, ttdh_051_1, ttdh_051_2, tmax_051, prmspringverlengd051, hverlengd051,          NG,           NG, mTDHd051,
	IH[hopdrempelen05] ? 1 : 1, d053,   t053_1,   t053_2, ttdh_053_1, ttdh_053_2, tmax_053, prmspringverlengd053, hverlengd053,          NG,           NG, mTDHd053,
	IH[hopdrempelen05] ? 1 : 1, d055,   t055_1,   t055_2, ttdh_055_1, ttdh_055_2, tmax_055, prmspringverlengd055, hverlengd055,          NG,           NG, mTDHd055,
	IH[hopdrempelen05] ? 1 : 1, d057,   t057_1,   t057_2, ttdh_057_1, ttdh_057_2, tmax_057, prmspringverlengd057, hverlengd057,          30,           30, mTDHd057,
	IH[hopdrempelen05] ? 1 : 2, d052,   t052_1,   t052_2, ttdh_052_1, ttdh_052_2, tmax_052, prmspringverlengd052, hverlengd052,          NG,           NG, mTDHd052,
	IH[hopdrempelen05] ? 1 : 2, d054,   t054_1,   t054_2, ttdh_054_1, ttdh_054_2, tmax_054, prmspringverlengd054, hverlengd054,          NG,           NG, mTDHd054,
	IH[hopdrempelen05] ? 1 : 2, d056,   t056_1,   t056_2, ttdh_056_1, ttdh_056_2, tmax_056, prmspringverlengd056, hverlengd056,          NG,           NG, mTDHd056,
	IH[hopdrempelen05] ? 1 : 2, d058,   t058_1,   t058_2, ttdh_058_1, ttdh_058_2, tmax_058, prmspringverlengd058, hverlengd058,          30,           30, mTDHd058,
	END);

	Voor het rijstrook-element: wanneer IH[hopdrempelen05] == TRUE wordt de waarde '1' gebruikt; wanneer het hulpelement niet waar is10-11	, wordt
	het rijstrooknummer gebruikt.
	Het hulpelement kan zowel met een schakelaar worden opgezet als vanuit de regelapplicatie.


*/
#if !defined AUTOMAAT || defined VISSIM   /* nog verwijderen */
/* definitie tbv voorkomen compiler warning */
extern int xyprintf(int x, int y, const char * szFormat, ...);
#endif

static int eavl[FCMAX][5];
static int detstor[FCMAX];

void hiaattijden_verlenging(bool nietToepassen, bool vrijkomkop, count mmk, count fc, ...)
{
	va_list argpt;                                    /* variabele argumentenlijst                                 */
	count dpnr;                                       /* arraynummer detectie-element                              */
	count t1, t2, tdh1, tdh2, tmax, tvag4_1, tvag4_2; /* arraynummers tijdelementen                                */
	count prmdetvw;                                   /* arraynummer parameter detectorvoorwaarden                 */
	count hevlvw;                                     /* arraynummer hulpelement extra verlengvoorwaarde           */
	count msthiaat;                                   /* arraynummer mem.element tbv bewaren statische hiaattijd   */
	count rijstrook_old = -1;                         /* vorige rijstrooknummer                                    */
	count rijstrook;                                  /* rijstrooknummer                                           */
	count max_rijstrook = 1;                          /* hoogste rijstrooknummer                                   */
	bool svw, vvw, evlvw, daft, svwG, hulp_bit3, verlengen[5], tdh_saw[5];
	count dp_teller = 0;                                /* telt aantal lussen vanaf stopstreep op bepaalde rijstrook */

	if (nietToepassen) return;

	/* initialisatie */
	for (rijstrook = 0; rijstrook < 5; rijstrook++)
	{
		verlengen[rijstrook] = FALSE;
		tdh_saw[rijstrook] = FALSE;                     /* TDH stroomafwaarts                                        */
		eavl[fc][rijstrook] = 0;                        /* eerste aktieve verlenglus (1 = eerste lus)                */
	}

	if (TRG[fc])  detstor[fc] = FALSE;                /* reset aanwezigheid detectiestoring voor deze fc bij TRG[] */ /*-*/

	/* vaststellen detectiestoring, alleen tijdens RV[] */ /*-*/
	if (RV[fc]) {
		va_start(argpt, fc);                            /* start var. argumentenlijst                                */
		do {
			rijstrook = va_arg(argpt, va_count);          /* lees rijstrooknummer                                      */
			if (rijstrook >= 0) {
				dpnr = va_arg(argpt, va_count);          /* lees array-nummer detectie                               */
				t1 = va_arg(argpt, va_count);          /* ongebruikt                                               */
				t2 = va_arg(argpt, va_count);          /* ongebruikt                                               */
				tdh1 = va_arg(argpt, va_count);          /* ongebruikt                                               */
				tdh2 = va_arg(argpt, va_count);          /* ongebruikt                                               */
				tmax = va_arg(argpt, va_count);          /* ongebruikt                                               */
				prmdetvw = va_arg(argpt, va_count);          /* ongebruikt                                               */
				hevlvw = va_arg(argpt, va_count);          /* ongebruikt                                               */
		//      svw      = va_arg(argpt, va_bool);           /* ongebruikt                                               */
		//      vvw      = va_arg(argpt, va_bool);           /* ongebruikt                                               */
		//      evlvw    = va_arg(argpt, va_bool);           /* ongebruikt                                               */ /*-*/
		//      daft     = va_arg(argpt, va_bool);           /* ongebruikt                                               */ /*-*/
		//      svwG     = va_arg(argpt, va_bool);           /* ongebruikt                                               */ /*-*/
				tvag4_1 = va_arg(argpt, va_count);          /* ongebruikt                                               */ /*-*/
				tvag4_2 = va_arg(argpt, va_count);          /* ongebruikt                                               */ /*-*/
				msthiaat = va_arg(argpt, va_count);          /* lees array-nummer MM tbv opslaan statisch hiaat          */ /*-*/

				if (!TRG[fc] && !ERV[fc]) {
#if defined (DL_type) && !defined (NO_DDFLUTTER) /* CCOL7 of hoger */  
					if (CIF_IS[dpnr] >= CIF_DET_STORING /*|| OG[dpnr]*/ || BG[dpnr] || FL[dpnr])   detstor[fc] |= TRUE;
#else
					if (CIF_IS[dpnr] >= CIF_DET_STORING /*|| OG[dpnr]*/ || BG[dpnr])               detstor[fc] |= TRUE;
#endif

#if !defined AUTOMAAT || defined VISSIM  /* nog verwijderen */
					xyprintf(125, 10, "detstor");
					xyprintf(125, 10 + 2 * fc + rijstrook, "     %d", detstor[fc]);
#endif
				}

				/* tijdens R[] statische hiaten gebruiken tbv default TLCGen detectieopvang; det.storing resetten op TRG[] */ /*-*/
				if (!TRG[fc]) {
					MM[msthiaat] = TDH_max[dpnr];              /* bewaar oorspronkelijke statische hiaattijd in MM          */
				}

			}
		} while (rijstrook >= 0);
		va_end(argpt);                     /* maak var. arg-lijst leeg */
	}

	if (RA[fc] && !SRA[fc] || G[fc] || GL[fc]) {        /*-*/
		va_start(argpt, fc);                              /* start var. argumentenlijst                                */
		do {
			rijstrook = va_arg(argpt, va_count);            /* lees rijstrooknummer                                      */
			if (rijstrook >= 0 && (detstor[fc] != TRUE)) {
				dpnr = va_arg(argpt, va_count);           /* lees array-nummer detectie                                */
				t1 = va_arg(argpt, va_count);           /* lees array-nummer tijdelement - moment 1                  */
				t2 = va_arg(argpt, va_count);           /* lees array-nummer tijdelement - moment 2                  */
				tdh1 = va_arg(argpt, va_count);           /* lees array-nummer tijdelement - hiaattijd 1               */
				tdh2 = va_arg(argpt, va_count);           /* lees array-nummer tijdelement - hiaattijd 2               */
				tmax = va_arg(argpt, va_count);           /* lees array-nummer tijdelement - maximum groentijd         */
				prmdetvw = va_arg(argpt, va_count);           /* lees array-nummer parameter   - detectorvoorwaarden       */
				hevlvw = va_arg(argpt, va_count);           /* lees array-nummer hulpelement - extra verlengvoorwaarde   */
		//      svw      = va_arg(argpt, va_bool);            /* lees springvoorwaarde  (op start groen, als er geen hiaatmeting      is op         de stroomafwaartse lussen, meteen naar de 2e/lagere hiaattijd overgaan) */
		//      vvw      = va_arg(argpt, va_bool);            /* lees verlengvoorwaarde (op start groen, als er geen hiaatmeting meer is op deze en de stroomafwaartse lussen, de verlengfunctie uitschakelen             ) */
		//      evlvw    = va_arg(argpt, va_bool);            /* lees extra verlengvoorwaarde (bijvoorbeeld tijdens aanwezig deelconflict                                  ) */ /*-*/
		//      daft     = va_arg(argpt, va_bool);            /* lees aftelvoorwaarde (op SG, indien wel TDH op deze lus maar niet stroomafwaarts, starten met aftellen TDH) */ /*-*/
		//      svwG     = va_arg(argpt, va_bool);            /* lees springvoorwaarde tijdens groen (zodra hiaat valt, doorstappen naar volgende lus; NIET geimplementeerd) */ /*-*/
				tvag4_1 = va_arg(argpt, va_count);           /* lees veiligheidsgroen 1e timer                            */ /*-*/
				tvag4_2 = va_arg(argpt, va_count);           /* lees veiligheidsgroen 2e timer                            */ /*-*/
				msthiaat = va_arg(argpt, va_count);           /* lees array-nummer MM tbv opslaan statisch hiaat           */ /*-*/

				/* omzetten parameter verlengvoorwaarden naar booleanse criteria */           /*-*/
				svw = PRM[prmdetvw] & BIT0;                  /* springvoorwaarde  (op SG[])     */
				vvw = PRM[prmdetvw] & BIT1;                  /* verlengvoorwaarde (op SG[])     */
				evlvw = (PRM[prmdetvw] & BIT2 || IH[hevlvw]);   /* extra verlengvoorwaarde         */
				daft = PRM[prmdetvw] & BIT3;                  /* direct aftelen voorwaarde       */
				svwG = PRM[prmdetvw] & BIT4;                  /* spring-tijdens-groen voorwaarde */

				/* afbreken lopende hiaattimers op start geel */
				AT[t1] = AT[t2] = AT[tmax] = SGL[fc];

				if (GL[fc]) {
					TDH_max[dpnr] = MM[msthiaat];             /* zet oorspronkelijke statische hiaattijden terug                  */
				}

				max_rijstrook = rijstrook;                    /* onthoud hoogste rijstrooknummer                                  */
				if (rijstrook != rijstrook_old) {
					eavl[fc][rijstrook] = 0;
					dp_teller = 0;
				}
				dp_teller++;
				rijstrook_old = rijstrook;

				if (T_max[tmax] == 0)       T_max[tmax] = (TFG_max[fc] + TVG_max[fc]);          /*-*/ /* overnemen max groentijd      */

				/* actuele hiaattijd bepalen */
				RT[t1] =
					RT[t2] =
					RT[tmax] = SG[fc]
					|| (vrijkomkop && eavl[fc][rijstrook] == 1 && (T_timer[tmax] == 0)); /*-*/

				if (ERA[fc])             TDH_max[dpnr] = T_max[tdh1];                       /*-*/ /* ERA[] ipv !G[]               */
				if (G[fc] && ET[t2])  TDH_max[dpnr] = T_max[tdh2];
				if (G[fc] && !RT[t1] && !T[t1] && T[t2]) {
					/* hiaattijd wijzigt tussen t1 en t2 lineair, van tdh1 naar tdh2 */
					/* ---y------ = -------------x----------- * -----------------richtingscoëfficiënt=a-------------- + ----b------ */
					TDH_max[dpnr] = (T_timer[t2] - T_max[t1]) * (T_max[tdh2] - T_max[tdh1]) / (T_max[t2] - T_max[t1]) + T_max[tdh1];
				}

				/* bepalen of er stroomafwaarts van een lus hiaattijden lopen */  /*-*/ /* locatie aangepast (vóór bepaling of meteen naar 2e hiaattijd gesprongen wordt) */
				if (TDH[dpnr]) {
					tdh_saw[rijstrook] = TRUE;
				}

				/* Als er geen verkeer is op de lus en de lussen ervoor bij start groen, meteen naar de 2e hiaattijd springen */
				if (SG[fc] && !tdh_saw[rijstrook] && dp_teller > 1 && svw) {
					RT[t1] =
						RT[t2] = FALSE;
					TDH_max[dpnr] = T_max[tdh2];
					if (vvw) {
						RT[tmax] = FALSE;
					}
				}

				/* Er mag verlengd worden op deze lus tot de timer is afgelopen     */           /* klopt dit? verlengen evlw nog testen! hier lijkt per rijstrook. */
				/* of wanneer extra verlengvoorwaarde evlvw aanwezig is             */
				if ((RT[tmax] || T[tmax] || evlvw) && G[fc] && TDH[dpnr]) {                                /*-*/ /* evlvw toegevoegd            */
					verlengen[rijstrook] = TRUE;
				}

				/* Bepaal eerste actieve verlenglus, vanaf de stopstreep gerekend   */
				if ((RT[tmax] || T[tmax]            /* maximum tijd loopt nog voor deze detector                         */
					|| evlvw) &&     /* of extra verlengvoorwaarde is aktief                              */ /*-*/
					G[fc] &&     /* signaalgroep is groen                                             */
		//          TDH[dpnr]                &&     /* hiaattijd van de detector loopt                                   */ /* uitgecommentaard in Goudappel code */
					eavl[fc][rijstrook] == 0) {  /* eerste actieve verlenglus is op deze rijstrook nog niet ingesteld */
					eavl[fc][rijstrook] = dp_teller;
				}

				/* afkappen maxtimer t.b.v. springen naar eerstvolgende lus stroomopwaarts*/ /*-*/
				if (G[fc] &&
					((dp_teller == 1) && vrijkomkop && !TDH[dpnr])  /* voor koplus bij niet (meer) aanwezig zijn van hiaat                    */
					||
					((dp_teller != 1) && svwG && !TDH[dpnr] &&      /* voor overige lussen indien spring-tijdens-groen voor deze lus waar is, */
					(eavl[fc][rijstrook] == dp_teller))) {  /* en de lus aktief is, en het hiaat voor de eerste keer gevallen is      */
					RT[tmax] = FALSE;
					AT[tmax] = TRUE;
				}

				/* Tijdens groen, als deze lus is de aktieve verlenglus, en stroomafwaarts lopen geen hiaattijden, en de t1 timer */ /*-*/
				 /* loopt nog, dan meteen hiaattijd laten aftellen door timers t1 en t2 gelijk te maken aan T_max[t1]              */
				 /* Aftellen gebeurt via eerdere formule y = x * a + b                                                             */
				if (daft && G[fc] && tdh_saw[rijstrook] && (eavl[fc][rijstrook] == dp_teller) && (T_timer[t1] < T_max[t1]) && TDH[dpnr]) { /*-*/
					T_timer[t1] = T_max[t1];
					T_timer[t2] = T_max[t1];
				}


#if !defined AUTOMAAT || defined VISSIM /* nog verwijderen */
				xyprintf(30, 10 + 0, "   dp   tdh   t1   t2   h1   h2    max  tmmax TDHsaw  dp ");
				xyprintf(30, 10 + 1 + dpnr, "  %s   %3d  %3d  %3d  %3d  %3d    %3d   %3d   %3d  %3d ", D_code[dpnr], TDH_max[dpnr], T_timer[t1], T_timer[t2], T_max[tdh1], T_max[tdh2],
					T_max[tmax], T_timer[tmax], tdh_saw[rijstrook], dp_teller);
				xyprintf(90, 10, "fc   rijstr   eavl   verl   MMmk ");
				xyprintf(90, 10 + 2 * fc + rijstrook, "%s     %3d    %3d      %d  %5d", FC_code[fc], rijstrook, eavl[fc][rijstrook], verlengen[rijstrook], MM[mmk]);
#endif


				/* afhandeling veiligheidsgroen, opgenomen in hoofdfunctie */ /*-*/
				if ((tvag4_1 != NG) && (tvag4_2 != NG) && SD[dpnr] && CIF_IS[dpnr] < CIF_DET_STORING) {
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
				else if ((tvag4_1 != NG) && (tvag4_2 != NG)) {
					RT[tvag4_1] = FALSE;
					RT[tvag4_2] = FALSE;
				}

				if ((tvag4_1 != NG) && (tvag4_2 != NG) &&
					(!(RT[tvag4_1] || T[tvag4_1]) ||
						!(RT[tvag4_2] || T[tvag4_2]))) {            /* minder dan 2 voertuigen in dilemmazone */
					YM[fc] &= ~BIT2;                                /* reset veiligheidsgroen                 */
				}
				/* einde afhandeling veiligheidsgroen */
			}
		} while (rijstrook >= 0);
		va_end(argpt);                     /* maak var. arg-lijst leeg */
	}

	hulp_bit3 = FALSE;
	for (rijstrook = 1; rijstrook <= max_rijstrook; rijstrook++)   /* voor alle rijstroken van de betreffende signaalgroep */
	{
		if (verlengen[rijstrook]) {
			hulp_bit3 = TRUE;
		}
		else
		{
			switch (rijstrook) {
			case 1:
				MM[mmk] &= ~(BIT1 | BIT2);    /* reset meetkriterium rijstrook 1 */  /*-*/ /* MM[mmk] ipv MK[fc] */
				break;
			case 2:
				MM[mmk] &= ~(BIT4 | BIT5);    /* reset meetkriterium rijstrook 2 */  /*-*/
				break;
			case 3:
				MM[mmk] &= ~(BIT6 | BIT7);    /* reset meetkriterium rijstrook 3 */  /*-*/
				break;
			case 4:
				MM[mmk] &= ~(BIT8 | BIT9);    /* reset meetkriterium rijstrook 4 */  /*-*/
				break;
			}
		}
	}

	if (!hulp_bit3) {                  /* als er geen enkele strook is waarop verlengd mag worden, ook BIT3 resetten */
		MK[fc] &= ~BIT3;                 /* reset meetkriterium BIT 3 */
	}
}
