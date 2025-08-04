###### INFORMACIONI SISTEMI

*- seminarski rad -*

  ---------------- ------------------------------------------------------
  Tema:            Informacioni sistemi ByteBuddy

                   

  Studenti:        Mahir Tahirovic

                   

                   

                   

                   

                   
  ---------------- ------------------------------------------------------

Novi Pazar, 2024

SADRŽAJ

[1. Korisnički zahtev [1](#korisnički-zahtev)](#korisnički-zahtev)

[2. SSA -- Strukturna Sistem Analiza
[3](#ssa-strukturna-sistem-analiza)](#ssa-strukturna-sistem-analiza)

[I. Dijagram konteksta [3](#dijagram-konteksta)](#dijagram-konteksta)

[II. Prvi nivo dekompozicije
[3](#prvi-nivo-dekompozicije)](#prvi-nivo-dekompozicije)

[III. Drugi nivo dekompozicije (Registracija)
[4](#drugi-nivo-dekompozicije-registracija)](#drugi-nivo-dekompozicije-registracija)

[IV. Treći nivo dekompozicije
[7](#treći-nivo-dekompozicije)](#treći-nivo-dekompozicije)

[3. Dijagram dekompozicije
[8](#dijagram-dekompozicije)](#dijagram-dekompozicije)

[4. Rečnik podataka [8](#rečnik-podataka)](#rečnik-podataka)

[5. EER model [9](#eer-model)](#eer-model)

[6. Relacioni model [10](#relacioni-model)](#relacioni-model)

[Relacije: [10](#_Toc162826450)](#_Toc162826450)

# Korisnički zahtev

ByteBuddy je inovativna platforma stvorena za programere koji žele
deliti aktuelne isecke koda, povezivati se s kolegama i istraživati svet
programiranja u novom svetlu.

Prilikom registracije, korisnici popunjavaju formu s ličnim podacima, a
sistem im šalje email s linkom za aktivaciju naloga kako bi osigurao
sigurnost i autentičnost korisničkih naloga.

Korisnici mogu postavljati isečke koda kao svoje \"Code Snippets\" i
deliti ih sa zajednicom. Svaki isečak može sadržavati:

- Tekstualni prikaz koda.

- Opcionalni fajl ili sliku vezanu za kod.

- Programski jezik (odabran iz unapred definisane liste).

- Tagove (#SORT, #SEARCH) za lakšu pretragu i organizaciju.

Svaki isečak koda ima sopstvenu stranicu na kojoj su prikazani svi
detalji, uključujući broj lajkova, komentara i tagova. Korisnici mogu
lajkovati isečke koda i ostavljati komentare kako bi podržali autore i
diskutovali o rešenjima.

Na svojim isecima koda, korisnici mogu tagovati druge programere kako bi
ih uključili u razgovore ili ih obaveštavali o određenim delovima koda.
ByteBuddy omogućava pretragu postova po tagovima, čime olakšava
korisnicima pronalaženje relevantnih kodova i ideja.

ByteBuddy takođe podržava funkcionalnosti prijateljstava, omogućavajući
korisnicima da se povežu s drugim programerima slanjem zahteva za
prijateljstvo. Prihvatanje ili odbijanje zahteva praćeno je
odgovarajućim obaveštenjem.

Korisnici mogu direktno slati poruke jedni drugima, pružajući platformu
za razmenu ideja, postavljanje pitanja i suradnju na projektima.
ByteBuddy čuva podatke o porukama i omogućava praćenje komunikacije.

Pored toga, programeri mogu kreirati stranice popunjavanjem izveštaja
koji se šalju sistemu. Sistem zatim vraća obaveštenje o tome da li je
stranica uspešno kreirana. Kreirane stranice mogu se sviđati drugim
korisnicima.

U okviru korisničkih profila, ByteBuddy pamti podatke o korisnicima,
uključujući ime, prezime, datum rođenja, pol, mesto rođenja, adresu,
zanimanje i bračni status. Korisnici mogu uspostavljati različite vrste
prijateljstava, a mogućnost lajkovanja objava njihovih prijatelja pruža
dodatni način za izražavanje podrške i interesovanja.

ByteBuddy stvara dinamičnu zajednicu programera koja se fokusira na
deljenje koda, povezivanje s istomišljenicima i unapređenje veština
programiranja, uz dodatak mogućnosti detaljnog označavanja i pretrage
kodova.

# SSA -- Strukturna Sistem Analiza

Pre nego što počnemo da projektujemo informacioni sistem za neki realni
sistem potrebno je da uradimo detaljnu analizu tog sistema. U ovom
slučaju kao metod za analizu koristimo Strukturnu sistemsku analizu
(SSA) koja nam služi da relativno složen realni sistem prikažemo kao
skup jednostavnijih podsistema čije funkcionisanje možemo lakše da
shvatimo, a samim tim i implementiramo.

### Dijagram konteksta

![C:\\Users\\Mahir\\Desktop\\Untitled-1.png](media/image1.png){width="6.299305555555556in"
height="3.710528215223097in"}

### Prvi nivo dekompozicije

![C:\\Users\\Mahir\\Desktop\\Untitled-2.png](media/image2.png){width="6.299305555555556in"
height="3.783400043744532in"}

### Drugi nivo dekompozicije (Registracija)

Registarcija:

![](media/image3.png){width="6.299305555555556in"
height="3.2222222222222223in"}

> Pracenje:

![](media/image4.png){width="6.299305555555556in"
height="3.314583333333333in"}

> Razmena poruka:
>
> ![](media/image5.png){width="6.299305555555556in"
> height="3.4034722222222222in"}

Postavke:

![](media/image6.png){width="6.299305555555556in"
height="3.732638888888889in"}

> Kreiranje stranice:

![](media/image7.png){width="6.299305555555556in"
height="3.120138888888889in"}

### Treći nivo dekompozicije

![](media/image8.png){width="6.299305555555556in"
height="4.236805555555556in"}

![](media/image9.PNG){width="6.299305555555556in"
height="3.604861111111111in"}

# Dijagram dekompozicije

![](media/image10.png){width="6.299305555555556in"
height="3.1597222222222223in"}

# Rečnik podataka

+-----------------------------------------------------------------------------+
| **Korisnik**\<Id, Ime, Prezime, Datum Rodjenja, Pol, Mesto Rodjenja,        |
| Adresa, Bracno stanje\>                                                     |
+=========================+=========================+=========================+
| **Polje**               | **Tip**                 | **Ogranicenje**         |
+-------------------------+-------------------------+-------------------------+
| Id                      | Long                    | not null                |
+-------------------------+-------------------------+-------------------------+
| Ime                     | varchar(64)             | not null                |
+-------------------------+-------------------------+-------------------------+
| Prezime                 | varchar(64)             | not null                |
+-------------------------+-------------------------+-------------------------+
| Datum rodjenja          | Date                    |                         |
+-------------------------+-------------------------+-------------------------+
| Pol                     | Char                    |                         |
+-------------------------+-------------------------+-------------------------+
| Mesto rodjenja          | varchar(255)            |                         |
+-------------------------+-------------------------+-------------------------+
| Adresa                  | varchar(255)            |                         |
+-------------------------+-------------------------+-------------------------+
| Bracno stanje           | varchar(64)             |                         |
+-------------------------+-------------------------+-------------------------+

+-----------------------------------------------------------------------------+
| **Objava**\<IdObjave, Naslov, Opis, DatumKreiranja, {korisnik}, Tag\>       |
+=========================+=========================+=========================+
| **Polje**               | **Tip**                 | **Ogranicenja**         |
+-------------------------+-------------------------+-------------------------+
| IdObajve                | Long                    | Not null                |
+-------------------------+-------------------------+-------------------------+
| Naslov                  | varchar(255)            |                         |
+-------------------------+-------------------------+-------------------------+
| Opis                    | varchar(1000)           |                         |
+-------------------------+-------------------------+-------------------------+
| DatumKreiranja          | Date                    |                         |
+-------------------------+-------------------------+-------------------------+
| Korisnik                | **Korisnik**            |                         |
+-------------------------+-------------------------+-------------------------+
| Tag                     | **Tag**                 |                         |
+-------------------------+-------------------------+-------------------------+

+-----------------------------------------------------------------------------+
| **Poruka**\<posaljilac, primalac, sadrzaj, datum, status\>                  |
+=========================+=========================+=========================+
| **Polje**               | **Tip**                 | **Ogranicenja**         |
+-------------------------+-------------------------+-------------------------+
| Posaljilac              | Korisnik                | Not null                |
+-------------------------+-------------------------+-------------------------+
| Korisnik                | Korisnik                | Not null                |
+-------------------------+-------------------------+-------------------------+
| Sadrzaj                 | Varchar(128)            | Not null                |
+-------------------------+-------------------------+-------------------------+
| Datum                   | Date                    | Not null                |
+-------------------------+-------------------------+-------------------------+
| Stauts                  | Delivered (Bolean)      |                         |
+-------------------------+-------------------------+-------------------------+

+-----------------------------------------------------------------------------+
| **Prijatelj** {korisnik}, {korisnik}, Inicijator, Status, Datum \>          |
+=========================+=========================+=========================+
| **Polje**               | **Tip**                 | **Ogranicenja**         |
+-------------------------+-------------------------+-------------------------+
| Korisnik                | **Korisnik**            | Not Null                |
+-------------------------+-------------------------+-------------------------+
| KorisnikB               | **Korisnik**            | Not null                |
+-------------------------+-------------------------+-------------------------+
| Inicijator              | **Korisnik**            | Not null                |
+-------------------------+-------------------------+-------------------------+
| Status                  | **Bolean**              |                         |
+-------------------------+-------------------------+-------------------------+
| Datum                   | **Date**                |                         |
+-------------------------+-------------------------+-------------------------+

+-----------------------------------------------------------------------------+
| **Stranica**\<IdStranice, Naziv, Opis, DatumKreiranja, {Korisnik}\>         |
+=========================+=========================+=========================+
| **Polje**               | **Tip**                 | **Ogranicenja**         |
+-------------------------+-------------------------+-------------------------+
| IdStranice              | Long                    | Not null                |
+-------------------------+-------------------------+-------------------------+
| Naziv                   | Varchar(128)            | Not null                |
+-------------------------+-------------------------+-------------------------+
| Opis                    | Varchar(512)            | Not null                |
+-------------------------+-------------------------+-------------------------+
| DatumKreiranja          | Date                    | Not null                |
+-------------------------+-------------------------+-------------------------+
| Korisni                 | Korisni                 |                         |
+-------------------------+-------------------------+-------------------------+

+-----------------------------------------------------------------------------+
| **Svidja**\<{Stranica}, {Objava}, {Korisnik}, DatumSvidjanja\>              |
+=========================+=========================+=========================+
| **Polje**               | **Tip**                 | **Ogranicenja**         |
+-------------------------+-------------------------+-------------------------+
| Stranica                | **Stranica**            | Not null                |
+-------------------------+-------------------------+-------------------------+
| Objava                  | **Objava**              |                         |
+-------------------------+-------------------------+-------------------------+
| Korisnik                | **Korisnik**            |                         |
+-------------------------+-------------------------+-------------------------+
| DatumSvidjanja          | **Date**                |                         |
+-------------------------+-------------------------+-------------------------+

+-----------------------------------------------------------------------------+
| **Tag**\<IdTaga, Ime, Oblast\>                                              |
+=========================+=========================+=========================+
| **Polje**               | **Tip**                 | **Ogranicenja**         |
+-------------------------+-------------------------+-------------------------+
| IdTaga                  | **Long**                | Not null                |
+-------------------------+-------------------------+-------------------------+
| Ime                     | Varchar(64)             | Not null                |
+-------------------------+-------------------------+-------------------------+
| Oblast                  | Varchar(128)            |                         |
+-------------------------+-------------------------+-------------------------+

+-----------------------------------------------------------------------------+
| **Tagovan**\<{Korisnik}, {Objava}, DatumTagovanja\>                         |
+=========================+=========================+=========================+
| **Polje**               | **Tip**                 | **Ogranicenja**         |
+-------------------------+-------------------------+-------------------------+
| Korisnik                | Korisnik                | Not null                |
+-------------------------+-------------------------+-------------------------+
| Objava                  | Objava                  | Not null                |
+-------------------------+-------------------------+-------------------------+
| DatumTagovanja          | Date                    |                         |
+-------------------------+-------------------------+-------------------------+

# EER model 

![](media/image11.png){width="6.299305555555556in"
height="4.0159722222222225in"}

# Relacioni model

Relacioni model pravimo na osnovu PMOV-a tako što se pridržavamo nekih
pravila. Prvo prebacujemo objekte čija kardinalnost ima gornju granicu M
Relacije koje im odgovaraju imaju iste atribute kao ti objekti, a
identifikator objekta je ključ u relaciji. Kod objekata čija je gornja
kardinalnost 1 ubacujemo još jedan atribut koji zovemo spoljnji ključ i
koji nam služi da se povežemo sa primarnim ključem neke tabele. Kada
slabe objekte prevodimo u relacije spuštamo ključ iz relacije koja je
nastala od jakog objekta u relaciju koja je nastala od slabog objekta i
dobijamo složen ključ koji se sastoji od ključa jakog objekta i još
jednog atributa slabog objekta. Šeme relacija su sledeće:

### **Relacioni model:**

1.  **Korisnik**\
    (Id, Ime, Prezime, DatumRodjenja, Pol, MestoRodjenja, Adresa,
    BracnoStanje)

    - **Primarni ključ:** Id

2.  **Stranica**\
    (IdStranice, Naziv, Opis, DatumKreiranja, VlasnikId)

    - **Primarni ključ:** IdStranice

    - **Spoljni ključ:** VlasnikId referencira Korisnik(Id)

3.  **Objava**\
    (IdObjave, Naslov, Opis, StranicaId)

    - **Primarni ključ:** IdObjave

    - **Spoljni ključ:** StranicaId referencira Stranica(IdStranice)

4.  **Tag**\
    (IdTaga, Ime, Oblast)

    - **Primarni ključ:** IdTaga

5.  **Sadrži**\
    (IdObjave, IdTaga)

    - **Primarni ključ:** (IdObjave, IdTaga)

    - **Spoljni ključevi:**

      - IdObjave referencira Objava(IdObjave)

      - IdTaga referencira Tag(IdTaga)

6.  **Taguje**\
    (KorisnikId, IdObjave, DatumTagovanja)

    - **Primarni ključ:** (KorisnikId, IdObjave)

    - **Spoljni ključevi:**

      - KorisnikId referencira Korisnik(Id)

      - IdObjave referencira Objava(IdObjave)

7.  **Prijatelj**\
    (PoslaoId, UzvratioId, InicijatorId, Status, Datum)

    - **Primarni ključ:** (PoslaoId, UzvratioId)

    - **Spoljni ključevi:**

      - PoslaoId referencira Korisnik(Id)

      - UzvratioId referencira Korisnik(Id)

      - InicijatorId referencira Korisnik(Id)

### **Ograničenja -- Spoljašnji ključevi:**

1.  Korisnik

    1.  Id je primarni ključ.

2.  Stranica

    1.  VlasnikId referencira Korisnik(Id).

3.  Objava

    1.  StranicaId referencira Stranica(IdStranice).

4.  Sadrži

    1.  IdObjave referencira Objava(IdObjave).

    2.  IdTaga referencira Tag(IdTaga).

5.  Taguje

    1.  KorisnikId referencira Korisnik(Id).

    2.  IdObjave referencira Objava(IdObjave).

6.  Prijatelj

    1.  PoslaoId referencira Korisnik(Id).

    2.  UzvratioId referencira Korisnik(Id).

    3.  InicijatorId referencira Korisnik(Id).
