   M S B A M L   `   `   `  ����C  ?Frank UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:Frank_UIFrank_UI  5   	       Frank_UI.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   &locclr-namespace:Frank_UI     5   	     � Title$  
Dispo-Tool��5   	   $
��1194��6   $
��1108��6*    � WindowState$ 	Maximized=�67    � 	Resources 5       �� Source�� 5   
   $ mResources.xaml?�6   %    .��5	      �  � RowDefinitions 5
   
   �� 5      $��30 �6   �� 5      $
��233* �6   �� 5      $	��375��6   $	��375 �6,   �� 5      $��24 �6   .+�5   
   �� -   5        $��gspHorizontal��5   @   $��5��6   $��Stretch=�6"   $	��Top=�6U    �� ResizeDirection$
 Auto=�6m   $��2��6�   �� 5   
   -   5        $��
statusBar1��5   K   $��3��6   $��26��6!   $��Stretch=�6-   $��Bottom=�6]   .@�5      ��  �� Orientation$ 
Horizontal=�6   $��Stretch=�62   $
��5, 0q�6N   .+�5      �� .��6    Anzahl Verkaufsaufträge:6:   �� 5      -   5        $��lblNumberOfSales��5   4   $��
0, 0, 5, 0q�6    .��6L   -6O   5      �� 5      $ 
Horizontal=�6   $��Stretch=�62   $
��5, 0q�6N   .+�5      �� .��6    Anzahl Artikel:61   �� 5      -   5        $��lblNumberofItems��5   4   $��
0, 0, 5, 0q�6    .��6L   -6O   5      �� 5      $ 
Horizontal=�6   $��Stretch=�62   $
��5, 0q�6N   .+�5      �� .��6    Letzte Aktualisierung:68   �� 5      -   5        $��lblLastUpdate��5   4   $��
0, 0, 5, 0q�6    .��6I   -6L   5      5      � 5    
   $��2��6   $��
0, 5, 0, 0q�6   $��Stretch=�60   $��Stretch=�6N      Contrast#��[  6j   .+�5!      �� -   5        $��lblInfo��5!   V   $��Informationen��6   $��28��6,   $��Stretch=�68   $	��Top=�6e   $��Center=�6}   �� 5"      -   5        $��lblArtikelnr��5"   t   $"��Artikelnummer + Beschreibung��6   $��28��6;   $
��Left=�6G   $��6,34,0,0q�6b   $	��Top=�6�   $��15(�6�   $
��Bold"�6�   �� 5$      -   5        $��label9��5$      $-��'Wird in folgenden Aufträgen benötigt:��6   $��28��6D   $
��Left=�6P   $��
576,65,0,0q�6k   $	��Top=�6�   &   System.Windows.Controls.DataGrid  5%      -	   5        $��dgrdInSales��5%   �      
IsReadOnly .    AlternatingRowBackground# [  6)   	   AutoGenerateColumns	 . $��Stretch=�6�   $��576,93,11,10q�6�   $��Stretch=�6�   
   GridLinesVisibility$

 None=�6�      CanUserResizeRows .  5'      5(      �� 5)   
   -
   5        $��	groupBox1��5)   T   $��	Allgemein��6   $
��Left=�6&   $��	6,73,0,10q�6A   $��Stretch=�6e   $	��325��6�   $��2��6�   .��5*      � .+�5+      �� -   5        $��label2��5+   o   $��Beschaffungsmethode:��6   $��28��67   $
��Left=�6C   $��6,6,0,0q�6^   $	��Top=�6}   �� 5,      -   5        $��lblBeschaffung��5,   v   $��Einkauf/Fertigungsauftrag��6   $��28��6<   $
��Left=�6H   $��	169,6,0,0q�6c   $	��Top=�6�   �� 5-      -   5        $��label4��5-   d   $��Einheit:��6   $��28��6+   $
��Left=�67   $��6,32,0,0q�6R   $	��Top=�6r   �� 5.      -   5        $��lblMengeBenoetigt��5.   c   $��10000��6   $��28��6(   $
��Left=�64   $��
169,58,0,0q�6O   $	��Top=�6|   �� 5/      -   5        $��label5��5/   k   $��Menge Benötigt:��6   $��28��62   $
��Left=�6>   $��6,58,0,0q�6Y   $	��Top=�6y   �� 50      -   5        $��
lblEinheit��50   a   $	��STK��6   $��28��6&   $
��Left=�62   $��
169,32,0,0q�6M   $	��Top=�6s   �� 51      -   5        $��label8��51   i   $��Lagerbestand:��6   $��28��60   $
��Left=�6<   $��6,84,0,0q�6W   $	��Top=�6w   �� 52      -   5        $��lblLagerbestand��52   c   $��10000��6   $��28��6(   $
��Left=�64   $��
169,84,0,0q�6O   $	��Top=�6z   �� 53      -   5        $��label10��53   h   $��Im Auftrag:��6   $��28��6.   $
��Left=�6:   $��	6,110,0,0q�6U   $	��Top=�6w   �� 54      -   5        $��lblImAuftrag��54   d   $��10000��6   $��28��6(   $
��Left=�64   $��169,110,0,0q�6O   $	��Top=�6x   �� 55      -   5        $��label12��55   h   $��Im Einkauf:��6   $��28��6.   $
��Left=�6:   $��	6,136,0,0q�6U   $	��Top=�6w   �� 56      -   5        $��lblImEinkauf��56   d   $��10000��6   $��28��6(   $
��Left=�64   $��169,136,0,0q�6O   $	��Top=�6x   �� 57      -   5        $��label14��57   j   $��In Fertigung:��6   $��28��60   $
��Left=�6<   $��	6,162,0,0q�6W   $	��Top=�6y   �� 58      -   5        $��lblInFertigung��58   d   $��10000��6   $��28��6(   $
��Left=�64   $��169,162,0,0q�6O   $	��Top=�6z   �� 59      -   5        $��label16��59   y   $"��In Fertigung in Komponenten:��6   $��28��6?   $
��Left=�6K   $��	6,188,0,0q�6f   $	��Top=�6�   �� 5:      -   5        $ ��lblInFertigungInKomponente��5:   d   $��10000��6   $��28��6(   $
��Left=�64   $��169,188,0,0q�6O   $	��Top=�6�   �� 5;      -   5        $��label18��5;   v   $��In Materialanforderungen:��6   $��28��6<   $
��Left=�6H   $��	6,214,0,0q�6c   $	��Top=�6�   �� 5<      -   5        $��lblInMaterialanforderung��5<   d   $��10000��6   $��28��6(   $
��Left=�64   $��169,214,0,0q�6O   $	��Top=�6�   �� 5=      $��Fehlende Menge:��6   $
��Left=�62   $��	6,240,0,0q�6M   $	��Top=�6`   �� 5>      -   5         ��Name$ lblMissingQuantity��5>   d   $��10000��6   $��28��6(   $
��Left=�64   $��169,240,0,0q�6O   $	��Top=�6�   5?      5@      �� 5A   
   -   5        $��label11��5A   j   $y�Hidden=�6   $��Suchen:��6$   $��1��66   $��28��6C   $
��Left=�6O   $	��Top=�6y   $��0,3,0,0q�6�   �� 5B   
   -   5        $��	txtSearch��5B   l   $y�Hidden=�6   $��1��6&   $��23��63   $
��Left=�6?   $��55,5,0,0q�6Z   $	��Top=�6}   $	��135��6�   6�   o� 5C   
   -    5        $��menu1��5C   9   $��23��6   $��Stretch=�6   $	��Top=�6F   .@�5D      m� $��_Datei��6   .@�5E      m�  m� Command$
 Stop��6   {�. 6=   5F      m� 5G      $��B_earbeiten��6   .@�5H      m� $ Search��6    m� IsCheckable . 6A   m� 5I      -!   5        $��mitmUpdateData��5I   8   $��Daten aktualisieren��6   6m   m� 5J      -"   5        $��mitmResetFilters��5J   =   $��Alle Filter zurücksetzen��6   6v   5K      m� 5L      $y�	Collapsed=�6   $��_Ansicht��6.   .@�5M      m� -#   5        $��mitmExpandAll��5M      $y�	Collapsed=�60   {�.  $��Alle Ausklappen��6Y   6�   m� 5N      -$   5        $��mitmCollapseAll��5N      $y�	Collapsed=�62   $��Alle Einklappen��6I   {�.  6�   5O      5P      �� 5Q   
   $��Stretch=�6   $��0,0,0,0q�64   $��1��6E   $��Stretch=�6R   .@�5R      �� $��Artikel��6   .��5S      � ���	#FFE5E5E5.+�5T        -%   5        $	��dgr��5T   �   $y�Visible=�6   # [  64    . $

 None=�6�   $��1��6�   $��0q�6�   	 .     Columns 5U        Frank_UI.mDataGridColumn  5V        Frank_UI.mDataGridColumn   FilterOptionsClick$" dgrAction_FilterOptionsClick��62      EnableFilter .    Caption$ Aktion��6x   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6�   ��  �� Path$ Action�6�     5W      $) #dgrReplenishment_FilterOptionsClick��62    . $ Beschaffungsmethode��6    6�   �� $ Replenishment System�6�     5X      $" dgrItemNo_FilterOptionsClick��62    . $ Artikelnummer��6x   ,  &System.Windows.Controls.DataGridColumn   SortMemberPath$	 No.��6�    6�   �� $ [No.]�  5Y      $' !dgrDescription_FilterOptionsClick��62    . $ Beschreibung��6}    6�   �� $ Description�  5Z      $( "dgrBenoetigtBis_FilterOptionsClick��62    . $  Benötigt bis Liefertermin��6~    6�   �� $# Erforderlich bis Liefertermin� �� StringFormat$ {0:dd.MM.yyyy}��  5[      $$ dgrMengeBis_FilterOptionsClick��62    . $ Menge bis Liefertermin��6z    6�   �� $ Menge bis Liefertermin�  5\      $$ dgrLocation_FilterOptionsClick��62    . $ Fertigungsstelle��6z    6�   �� $ Location�  5]      $$ dgrVendorNo_FilterOptionsClick��62    . $ Kreditor Nr.��6z    6�   �� $ 	VendorNo.�  5^      $&  dgrVendorName_FilterOptionsClick��62    . $ Kreditor Name��6|    6�   �� $ Vendor Name�5`      5a      5b      �� 5c      $��Verkaufsaufträge��6   .��5d      � ���	#FFE5E5E5.+�5e          Frank_UI.mDataGridTreeView  -&   5          Name$
 dgtv��5e   �       Frank_UI.mDataGridTreeView   TreeViewItem_Expanded$ trvItem_Expanded��6,      TreeViewItem_Collapsed$ trvItem_Collapsed��6U   $��1��6�   $��0q�6�      Columns 5f        5g      $' !clmDescription_FilterOptionsClick��6�      DisplayIndex$ 3��62      Width5  /System.Windows.Controls.DataGridLengthConverter$ 6* 6C   $ Beschreibung��6N      CellType$ TreeViewItemText��6e    . 6�     5h      $( "clmAvailability_FilterOptionsClick��6y   $ 0��62   $ * 6C   $ Verfügbarkeit��6M    . 6�     5i      $$ clmCustomer_FilterOptionsClick��6r   $ 2��62   $ 3* 6C   $ Kunde��6N    . 6�     5j      $  clmDate_FilterOptionsClick��6�   $ 1��62   $ * 6C   $ Liefertermin frühestens��6M    . 6�   5l      5m      5n      5o      �� 5p   
   -'   5         ��Name$ groupBox1_Copy��5p   U   $��Aktionen��6   $
��Left=�6%   $��336,73,0,10q�6@   $��Stretch=�6m   $	��232��6�   $��2��6�   .��5q      �� -(   5        $��	lblAction��5q      $�� ��6%   $	��Top=�60   $��6q�6H   5r      5s      5t      