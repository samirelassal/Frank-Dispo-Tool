   M S B A M L   `   `   `  ����C  ?Frank UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:Frank_UIFrank_UI  5   	       Frank_UI.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   &locclr-namespace:Frank_UI     5   	   -   5          � Title$  
Dispo-Tool��5   	   $
��1195��6   $
��1268��6*    � WindowState$ 	Maximized=�67    � 	Resources 5       �� Source�� 5   
   $ mResources.xaml?�6   %    .��5	      �  � RowDefinitions 5
   
   �� 5      $��30 �6   �� 5      $
��233* �6   �� 5      $	��150��6   $	��300 �6,   �� 5      $��24 �6   .+�5   
   �� -   5        $��gspHorizontal��5   @   $��5��6   $��Stretch=�6"   $	��Top=�6U    �� ResizeDirection$
 Auto=�6m   $��2��6�   �� 5   
   -   5        $��
statusBar1��5   K   $��3��6   $��26��6!   $��Stretch=�6-   $��Bottom=�6]   .@�5      ��  �� Orientation$ 
Horizontal=�6   $��Stretch=�62   $
��5, 0q�6N   .+�5      �� .��6    Verkaufsaufträge:63   �� 5      -   5        $��lblNumberOfSales��5   4   $��
0, 0, 5, 0q�6    .��6L   -6O   5      �� 5      $ 
Horizontal=�6   $��Stretch=�62   $
��5, 0q�6N   .+�5      �� .��6    
Artikel:6*   �� 5      -   5        $��lblNumberofItems��5   4   $��
0, 0, 5, 0q�6    .��6L   -6O   5      �� 5      $ 
Horizontal=�6   $��Stretch=�62   $
��5, 0q�6N   .+�5      �� .��6    Letzte Aktualisierung:68   �� 5      -   5        $��lblLastUpdate��5   4   $��
0, 0, 5, 0q�6    .��6I   -6L   5      �� 5      $ 
Horizontal=�6   $��Stretch=�62   $
��5, 0q�6N   .+�5       �� .��6    Minuten Fehlende Menge:69   �� 5!      -   5        $"��lblSumMinutesMissingQuantity��5!   4   $��
0, 0, 5, 0q�6    .��6�   -6�   5"      �� 5#      $ 
Horizontal=�6   $��Stretch=�62   $
��5, 0q�6N   .+�5$      �� .��6    Minuten Produzierbare Menge:6>   �� 5%      -   5        $%��lblSumMinutesProducableQuantity��5%   4   $��
0, 0, 5, 0q�6    .��6[   -6^   5&      �� 5'      $ 
Horizontal=�6   $��Stretch=�62   $
��5, 0q�6N   .+�5(      �� .��6    Euro Aktuell Fehlende Menge:6>   �� 5)      -	   5        $'��!lblSumEuroActuallyMissingQuantity��5)   4   $��
0, 0, 5, 0q�6    .��6]   -6`   5*      5+      � 5,   
   $��2��6   $��
0, 5, 0, 0q�6   $��Stretch=�60   $��Stretch=�6N      Contrast#��[  6j   .+�5-      �� -
   5        $��lblInfo��5-   V   $��Informationen��6   $��28��6,   $��Stretch=�68   $	��Top=�6e   $��Center=�6}   �� 5.      -   5        $��lblArtikelnr��5.   t   $"��Artikelnummer + Beschreibung��6   $��28��6;   $
��Left=�6G   $��6,34,0,0q�6b   $	��Top=�6�   $��15(�6�   $
��Bold"�6�   �� 50      -   5        $��label9��50      $-��'Wird in folgenden Aufträgen benötigt:��6   $��28��6D   $
��Left=�6P   $��
530,65,0,0q�6k   $	��Top=�6�   &   System.Windows.Controls.DataGrid  51      -   5        $��dgrdInSales��51   �      
IsReadOnly .    AlternatingRowBackground# [  6)   	   AutoGenerateColumns	 .  $��Stretch=�6�   $��530,93,380,10q�6�   $��Stretch=�6�   
   GridLinesVisibility$

 None=�6�      CanUserResizeRows .     Columns 52      0  *System.Windows.Controls.DataGridTextColumn  53      ,  &System.Windows.Controls.DataGridColumn   Header$ Menge��6)   1  +System.Windows.Controls.DataGridBoundColumn   Binding 68   �� *Menge+6T     54      $	 Nr.��6)    66   �� *Nr+6O     55      $ 
Priorität��6)      SortMemberPath$ PrioritätZahl��6Z      SortDirection$ 
DescendingX�6y    6<   �� *
Priorität+6y     56      $ Liefertermin Bestätigt��6)    6I   �� *ShipmentDate+ �� StringFormat$ {0:dd.MM.yy}��  57      $ Kunde��6)    68   �� *Kunde+  58      $ Artikel��6)    6:   �� *	Artikel+5:        5;      -   5        $��dgrdRequisition��5;   �    . # [  6)   $

 None=�6j    .  	 .  $��Right=�6�   $��0,93,215,10q�6�   $��Stretch=�6  $	��160��6    5<        5=      $ Erstellt Am��6)    6>   �� *Erstellt Am+$ {0:dd.MM.yy}��  5>      $ 	Lieferant��6)   $ 
Vendor No.��6b    6<   ��  �� Path$ [Vendor No.]�6b   5@      �� 5A      -   5         ��Name$ lblRequisition��5A   q   $��Materialanforderungen:��6   $��28��65   $��Right=�6A   $��
0,65,240,0q�6]   $	��Top=�6�     5B      -   5          Name$ dgrdPurchase��5B   �    . # [  6)   $

 None=�6j    .  	 .  $��Right=�6�   $��
0,93,10,10q�6�   $��Stretch=�6  $	��200��6   5C        5D      $ Erwarteter Eingang��6)    6E   �� *Expected Receipt Date+$ {0:dd.MM.yy}��  5E      $ 	Lieferant��6)   $ 
Vendor No.��6]    6<   �� *[Vendor No.]+6]   5G      �� 5H      -   5        $ lblPurchase��5H   h   $��Bestellungen:��6   $��28��6,   $��Right=�68   $��
0,65,129,0q�6T   $	��Top=�6}   5I      �� 5J   
   -   5        $��	groupBox1��5J   S   $��	Allgemein��6   $
��Left=�6&   $��6,73,0,7q�6A   $��Stretch=�6d   $	��300��6�   $��2��6�   .��5K      � .+�5L      �� -   5        $��lv��5L      $��Stretch=�6%   $��Stretch=�6C   ��5M      �� 5N       �� 
TargetType" ��6    .�5O      ��  �� Property � �� �� Value$ Stretch=�6K   5P       �� View 5R      �� 5S      ! �� ColumnHeaderContainerStyle 5T      �� 5U   "   " ��6(   .�5V   &   ��  � y�$ 	Collapsed=�6C   5W   #   .P�5Y      ��  �� Width$	 160��6-   $
��Name��69    �� DisplayMemberBinding 6G   �� *Name+�� 5Z      $	 120��6-   $
��Wert��69    6G   �� *Wert+5[      5]      5_      5`      �� 5a   
   -   5        $��label11��5a   j   $y�Hidden=�6   $��Suchen:��6$   $��1��66   $��28��6C   $
��Left=�6O   $	��Top=�6y   $��0,3,0,0q�6�   o� 5b   
   -   5        $��menu1��5b   9   $��23��6   $��Stretch=�6   $	��Top=�6F   .@�5c      m� $��B_earbeiten��6   .@�5d      m� -   5        $��mitmUpdateData��5d   8   $��Daten aktualisieren��6   6m   m� 5e      -   5        $��mitmResetFilters��5e   =   $��Alle Filter zurücksetzen��6   6v   5f      m� 5g      $��_Hilfe��6   .@�5h      m� -   5        $��mtmAbout��5h   4   $��Über Dispo-Tool��6   6]   m� 5i      -   5        $��	mtmManual��5i   -   $��Handbuch��6   6X   5j      m� 5k      $��Entwickler-Tools��6   .@�5l      m� -   5        $��mtmAdvancedFilter��5l   4   $��Advanced Filter��6   6o   5m      5n      �� 5o   
   $��Stretch=�6   $��0,0,0,0q�64   $��1��6E   $��Stretch=�6R   .@�5p      �� $��Artikel��6   .��5q      � ���	#FFE5E5E5.+�5r        -   5        $	��dgr��5r   �   $y�Visible=�6   # [  64    . $

 None=�6�   $��1��6�   $��0q�6�   	 .   5s        Frank_UI.mDataGridColumn  5t        Frank_UI.mDataGridColumn   FilterOptionsClick$" dgrAction_FilterOptionsClick��6E      FilterDeletedClick$! dgrAction_DeleteFilterClick��6w   -   5        	dgrAction��   EnableFilter .    Caption$ Aktion��5t   �    6�   �� $ Action�6�     5u      $) #dgrReplenishment_FilterOptionsClick��6L   $( "dgrReplenishment_DeleteFilterClick��6�   -   5        dgrReplenishment�� . $ Beschaffungsmethode��5u   �    6�   �� $ Replenishment System�6"    5v      $" dgrItemNo_FilterOptionsClick��6E   $! dgrItemNo_DeleteFilterClick��6w   -   5        	dgrItemNo�� . $ Artikelnummer��5v   �   $	 No.��6�    6�   �� $ [No.]�  5w      $' !dgrDescription_FilterOptionsClick��6J   $&  dgrDescription_DeleteFilterClick��6�   -   5        dgrDescription�� . $ Beschreibung��5w   �    6�   �� $ Description�  5x      $( "dgrNextPriority_FilterOptionsClick��6s   $' !dgrNextPriority_DeleteFilterClick��6�   -    5        dgrNextPriority��$ Nächste Priorität Zahl��5x   K    .  $ Nächste Priorität��6�    5y   "   `� 5z   &   $ {0}{1}��63   .1�5{   *   �� $$ Nächste Priorität Buchstaben�62   �� 5|   *   $ Nächste Priorität Zahl�62   5}   '   5        5�      $$ dgrLocation_FilterOptionsClick��6G   $# dgrLocation_DeleteFilterClick��6{   -!   5        dgrLocation�� . $ Fertigungsstelle��5�   �    6�   "   Frank_UI.CultureAwareBinding  $ Location�  5�      $' !dgrMitarbeiter_FilterOptionsClick��6�   $' !dgrMitarbeiter_FilterDeletedClick��6�   -"   5        dgrMitarbeiter�� . $ Mitarbeiter��5�   ^    6t   �� $ Mitarbeiter�6�     5�      $$ dgrVendorNo_FilterOptionsClick��6G   $# dgrVendorNo_DeleteFilterClick��6{   -#   5        dgrVendorNo�� . $ Kreditor Nr.��5�   �   $ 
Vendor No.��6�    6�   �� $ [Vendor No.]�  5�      $&  dgrVendorName_FilterOptionsClick��6I   $% dgrVendorName_DeleteFilterClick��6   -$   5        dgrVendorName�� . $ Kreditor Name��5�   �    6�   �� $ Vendor Name�  5�      -%   5        dgrQuantityAbleToProduce��    TextType$  Number=�5�   T   $ Fertigen��6f    6y     $ Quantity able to Produce�  5�      -&   5        dgrMissingQuantity��$  Number=�5�   N   $ Fehlende Menge��6`    6y     $ Missing Quantity�  5�      -'   5        dgrRecProdQuantity��$  Number=�5�   N   $ Empf. Fertigungsmenge��6`   $  Recommended Prod. Quantity��6�    6�     $" [Recommended Prod. Quantity]�  5�      -(   5        dgrMinutenProStueck��$  Number=�5�   O   $ Minuten pro Stück��6a    6}     $ Minuten pro Stück�  5�      -)   5        dgrMinutenFehlendeMenge��$  Number=�5�   S   $ Minuten Fehlende Menge��6e    6�     $ Minuten Fehlende Menge�  5�      -*   5         dgrMinutenProduzierbareMenge��$  Number=�5�   X   $! Minuten Produzierbare Menge��6j    6�     $! Minuten Produzierbare Menge�  5�      -+   5        dgrEuroProStueck��$  Currency=�5�   L   $ Euro Pro Stück��6`    6y     $ Euro pro Stück�$ c��  5�      -,   5        $ dgrEuroTatsaechlichFehlendeMenge��$  Currency=�5�   \   $! Euro Aktuell Fehlende Menge��6p    6�     $&  Euro Tatsächlich Fehlende Menge�$ c��!   ElementStyle! 5�   "   �� 5�   &   " ��6,   .�5�   *   ��  � ��$ Right=�6P   5�   '   5�      5�      5�      5�      �� 5�      $��Verkaufsaufträge��6   .��5�      � ���	#FFE5E5E5.+�5�          Frank_UI.mDataGridTreeView  --   5        "  Name$
" dgtv��5�   �    	   Frank_UI.mDataGridTreeView# 	  TreeViewItem_Expanded$# trvItem_Expanded��6,   ($ 	  !TreeViewItem_MouseRightButtonDown$,$ &dgtv_TreeViewItem_MouseRightButtonDown��6U   % 	  TreeViewItem_Collapsed$% trvItem_Collapsed��6�   & 	  LayoutUpdated$& dgtv_LayoutUpdated��6�   $��1��6�   $��0q�6�   '   Columns' 5�        5�      $' !clmDescription_FilterOptionsClick��6�   $' !clmDescription_FilterDeletedClick��6�   (   DisplayIndex$( 5��62   )   Width5
  /System.Windows.Controls.DataGridLengthConverter$) 6*
 6C   $ Beschreibung��6N   *   CellType$* TreeViewItemText��6e    . 6  4  .System.Windows.Controls.DataGridTemplateColumn  5�      $ [Available]��65   $( 0��6R   $ Verfügbarkeit��6c   +   CellTemplate+ 5�   "   �� 5�   &   .R�5�   *   �� ��6;   �� *[Available]+  BoolToCheckBox, �� 	Converter#, [ 5�   '   5�        5�      $$ clmCustomer_FilterOptionsClick��6r   $$ clmCustomer_FilterDeletedClick��6�   $( 2��62   $) 3*
 6C   $ Kunde��6N    . 6�     5�      $  clmDate_FilterOptionsClick��6�   $  clmDate_FilterDeletedClick��6�   $
  Date=�62   $( 1��6B   $) *
 6S   $ Liefertermin bestätigt��6]    . 6�     5�      $  Number=�62   $( 3��6D   $) *
 6U   $ Minuten Positionsmenge��6_   6�     5�      $  Currency=�62   $( 4��6F   $) *
 6W   $ Euro Positionsmenge��6a   6�     5�      $  Number=�62   $( 6��6D   $) *
 6U   $ 
Priorität��6_   6v   5�      5�      5�      5�      �� 5�   
   -.   5        - ��Name$- groupBox1_Copy��5�   T   $��Aktionen��6   $
��Left=�6%   $��
311,76,0,7q�6@   $��Stretch=�6l   $	��209��6�   $��2��6�   .��5�      �� -/   5        $��	lblAction��5�      $�� ��6%   $	��Top=�60   $��6q�6H   5�      5�      5�      