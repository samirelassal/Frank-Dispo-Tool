   M S B A M L   `   `   `  ����C  ?Frank UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:Frank_UIFrank_UI  5   	       Frank_UI.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   &locclr-namespace:Frank_UI     5   	     � Title$  
Dispo-Tool��5   	   $
��1195��6   $
��1268��6*    � WindowState$ 	Maximized=�67    � 	Resources 5       �� Source�� 5   
   $ mResources.xaml?�6   %    .��5	      �  � RowDefinitions 5
   
   �� 5      $��30 �6   �� 5      $
��233* �6   �� 5      $	��400��6   $	��400 �6,   �� 5      $��24 �6   .+�5   
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
0, 0, 5, 0q�6    .��6I   -6L   5      �� 5      $ 
Horizontal=�6   $��Stretch=�62   $
��5, 0q�6N   .+�5       �� .��6    Summe Minuten Fehlende Menge:6?   �� 5!      -   5        $"��lblSumMinutesMissingQuantity��5!   4   $��
0, 0, 5, 0q�6    .��6X   -6[   5"      �� 5#      $ 
Horizontal=�6   $��Stretch=�62   $
��5, 0q�6N   .+�5$      �� .��6    $"Summe Minuten Produzierbare Menge:6D   �� 5%      -   5        $%��lblSumMinutesProducableQuantity��5%   4   $��
0, 0, 5, 0q�6    .��6[   -6^   5&      �� 5'      $ 
Horizontal=�6   $��Stretch=�62   $
��5, 0q�6N   .+�5(      �� .��6    )'Summe Euro Tatsächlich Fehlende Menge:6H   �� 5)      -   5        $'��!lblSumEuroActuallyMissingQuantity��5)   4   $��
0, 0, 5, 0q�6    .��6]   -6`   5*      5+      � 5,   
   $��2��6   $��
0, 5, 0, 0q�6   $��Stretch=�60   $��Stretch=�6N      Contrast#��[  6j   .+�5-      �� -	   5        $��lblInfo��5-   V   $��Informationen��6   $��28��6,   $��Stretch=�68   $	��Top=�6e   $��Center=�6}   �� 5.      -
   5        $��lblArtikelnr��5.   t   $"��Artikelnummer + Beschreibung��6   $��28��6;   $
��Left=�6G   $��6,34,0,0q�6b   $	��Top=�6�   $��15(�6�   $
��Bold"�6�   �� 50      -   5        $��label9��50      $-��'Wird in folgenden Aufträgen benötigt:��6   $��28��6D   $
��Left=�6P   $��
530,65,0,0q�6k   $	��Top=�6�   &   System.Windows.Controls.DataGrid  51      -   5        $��dgrdInSales��51   �      
IsReadOnly .    AlternatingRowBackground# [  6)   	   AutoGenerateColumns	 .  $��Stretch=�6�   $��530,93,380,10q�6�   $��Stretch=�6�   
   GridLinesVisibility$

 None=�6�      CanUserResizeRows .     Columns 52      0  *System.Windows.Controls.DataGridTextColumn  53      ,  &System.Windows.Controls.DataGridColumn   Header$ Menge��6)   1  +System.Windows.Controls.DataGridBoundColumn   Binding 68   �� *Menge+6T     54      $	 Nr.��6)    66   �� *Nr+6O     55      $ 
Priorität��6)      SortMemberPath$ PrioritätZahl��6Z      SortDirection$ 
DescendingX�6y    6<   �� *
Priorität+6y     56      $ Kunde��6)    68   �� *Kunde+  57      $ Artikel��6)    6:   �� *	Artikel+59        5:      -   5        $��dgrdRequisition��5:   �    . # [  6)   $

 None=�6j    .  	 .  $��Right=�6�   $��0,93,215,10q�6�   $��Stretch=�6  $	��160��6    5;        5<      $ Erstellt Am��6)    6>   �� *Erstellt Am+ �� StringFormat$ {0:dd.MM.yy}��  5=      $ 	Lieferant��6)   $ 
Vendor No.��6b    6<   ��  �� Path$ [Vendor No.]�6b   5?      �� 5@      -   5         ��Name$ lblRequisition��5@   q   $��Materialanforderungen:��6   $��28��65   $��Right=�6A   $��
0,65,240,0q�6]   $	��Top=�6�     5A      -   5          Name$ dgrdPurchase��5A   �    . # [  6)   $

 None=�6j    .  	 .  $��Right=�6�   $��
0,93,10,10q�6�   $��Stretch=�6  $	��200��6   5B        5C      $# Erwartetes Wareneingangsdatum��6)    6P   �� *Expected Receipt Date+$ {0:dd.MM.yy}��5E      �� 5F      -   5        $ lblPurchase��5F   h   $��Bestellungen:��6   $��28��6,   $��Right=�68   $��
0,65,129,0q�6T   $	��Top=�6}   5G      �� 5H   
   -   5        $��	groupBox1��5H   S   $��	Allgemein��6   $
��Left=�6&   $��6,73,0,7q�6A   $��Stretch=�6d   $	��300��6�   $��2��6�   .��5I      � .+�5J      �� -   5        $��label2��5J   o   $��Beschaffungsmethode:��6   $��28��67   $
��Left=�6C   $��6,6,0,0q�6^   $	��Top=�6}   �� 5K      -   5        $��lblBeschaffung��5K   v   $��Einkauf/Fertigungsauftrag��6   $��28��6<   $
��Left=�6H   $��	187,6,0,0q�6c   $	��Top=�6�   $	��126��6�   �� 5L      -   5        $��label4��5L   d   $��Einheit:��6   $��28��6+   $
��Left=�67   $��6,32,0,0q�6R   $	��Top=�6r   �� 5M      -   5        $��lblMengeBenoetigt��5M   c   $��10000��6   $��28��6(   $
��Left=�64   $��
187,58,0,0q�6O   $	��Top=�6|   $��48��6�   �� 5N      -   5        $��label5��5N   k   $��Menge Benötigt:��6   $��28��62   $
��Left=�6>   $��6,58,0,0q�6Y   $	��Top=�6y   �� 5O      -   5        $��
lblEinheit��5O   a   $	��STK��6   $��28��6&   $
��Left=�62   $��
187,32,0,0q�6M   $	��Top=�6s   $��48��6�   �� 5P      -   5        $��label8��5P   i   $��Lagerbestand:��6   $��28��60   $
��Left=�6<   $��6,84,0,0q�6W   $	��Top=�6w   �� 5Q      $#��Übergeordneter Lagerbestand:��6   $��28��6?   $
��Left=�6K   $��	6,110,0,0q�6f   $	��Top=�6y   �� 5R      -   5        $ lblParentInventory��5R   d   $��10000��6   $��28��6(   $
��Left=�64   $��187,110,0,0q�6O   $	��Top=�6�   $��48��6�   �� 5S      -   5        $��lblLagerbestand��5S   c   $��10000��6   $��28��6(   $
��Left=�64   $��
187,84,0,0q�6O   $	��Top=�6z   $��48��6�   �� 5T      -   5        $��label10��5T   h   $��Im Auftrag:��6   $��28��6.   $
��Left=�6:   $��	6,136,0,0q�6U   $	��Top=�6w   �� 5U      -   5        $��lblImAuftrag��5U   d   $��10000��6   $��28��6(   $
��Left=�64   $��187,136,0,0q�6O   $	��Top=�6x   $��48��6�   �� 5V      -   5        $��label12��5V   h   $��Im Einkauf:��6   $��28��6.   $
��Left=�6:   $��	6,162,0,0q�6U   $	��Top=�6w   �� 5W      -   5        $��lblImEinkauf��5W   d   $��10000��6   $��28��6(   $
��Left=�64   $��187,162,0,0q�6O   $	��Top=�6x   $��48��6�   �� 5X      -   5        $��label14��5X   j   $��In Fertigung:��6   $��28��60   $
��Left=�6<   $��	6,188,0,0q�6W   $	��Top=�6y   �� 5Y      -    5        $��lblInFertigung��5Y   d   $��10000��6   $��28��6(   $
��Left=�64   $��187,188,0,0q�6O   $	��Top=�6z   $��48��6�   �� 5Z      -!   5        $��label16��5Z   y   $"��In Fertigung in Komponenten:��6   $��28��6?   $
��Left=�6K   $��	6,214,0,0q�6f   $	��Top=�6�   �� 5[      -"   5        $ ��lblInFertigungInKomponente��5[   d   $��10000��6   $��28��6(   $
��Left=�64   $��187,214,0,0q�6O   $	��Top=�6�   $��48��6�   �� 5\      -#   5        $��label18��5\   v   $��In Materialanforderungen:��6   $��28��6<   $
��Left=�6H   $��	6,240,0,0q�6c   $	��Top=�6�   �� 5]      -$   5        $��lblInMaterialanforderung��5]   d   $��10000��6   $��28��6(   $
��Left=�64   $��187,240,0,0q�6O   $	��Top=�6�   $��48��6�   �� 5^      $��Fehlende Menge:��6   $
��Left=�62   $��	6,266,0,0q�6M   $	��Top=�6`   �� 5_      -%   5        $ lblMissingQuantity��5_   d   $��10000��6   $��28��6(   $
��Left=�64   $��187,266,0,0q�6O   $	��Top=�6�   $��48��6�   5`      5a      �� 5b   
   -&   5        $��label11��5b   j   $y�Hidden=�6   $��Suchen:��6$   $��1��66   $��28��6C   $
��Left=�6O   $	��Top=�6y   $��0,3,0,0q�6�   o� 5c   
   -'   5        $��menu1��5c   9   $��23��6   $��Stretch=�6   $	��Top=�6F   .@�5d      m� $��B_earbeiten��6   .@�5e      m� -(   5        $��mitmUpdateData��5e   8   $��Daten aktualisieren��6   6m   m� 5f      -)   5        $��mitmResetFilters��5f   =   $��Alle Filter zurücksetzen��6   6v   5g      m� 5h      $��_Hilfe��6   .@�5i      m� -*   5        $��mtmAbout��5i   4   $��Über Dispo-Tool��6   6]   m� 5j      -+   5        $��	mtmManual��5j   -   $��Handbuch��6   6X   5k      5l      �� 5m   
   $��Stretch=�6   $��0,0,0,0q�64   $��1��6E   $��Stretch=�6R   .@�5n      �� $��Artikel��6   .��5o      � ���	#FFE5E5E5.+�5p        -,   5        $	��dgr��5p   �   $y�Visible=�6   # [  64    . $

 None=�6�   $��1��6�   $��0q�6�   	 .   5q        Frank_UI.mDataGridColumn  5r        Frank_UI.mDataGridColumn   FilterOptionsClick$" dgrAction_FilterOptionsClick��6E      FilterDeletedClick$! dgrAction_DeleteFilterClick��6w   --   5        	dgrAction��   EnableFilter .    Caption$ Aktion��5r   �    6�   �� $ Action�6�     5s      $) #dgrReplenishment_FilterOptionsClick��6L   $( "dgrReplenishment_DeleteFilterClick��6�   -.   5        dgrReplenishment�� . $ Beschaffungsmethode��5s   �    6�   �� $ Replenishment System�6"    5t      $" dgrItemNo_FilterOptionsClick��6E   $! dgrItemNo_DeleteFilterClick��6w   -/   5        	dgrItemNo�� . $ Artikelnummer��5t   �   $	 No.��6�    6�   �� $ [No.]�  5u      $' !dgrDescription_FilterOptionsClick��6J   $&  dgrDescription_DeleteFilterClick��6�   -0   5        dgrDescription�� . $ Beschreibung��5u   �    6�   �� $ Description�  5v      $( "dgrNextPriority_FilterOptionsClick��6s   $' !dgrNextPriority_DeleteFilterClick��6�   -1   5        dgrNextPriority��$ Nächste Priorität Zahl��5v   K    .  $ Nächste Priorität��6�    5w   "   `� 5x   &   $ {0}{1}��63   .1�5y   *   �� $$ Nächste Priorität Buchstaben�62   �� 5z   *   $ Nächste Priorität Zahl�62   5{   '   5}        5~      $$ dgrMengeBis_FilterOptionsClick��6G   $# dgrMengeBis_DeleteFilterClick��6{   -2   5        dgrMengeBis�� . $ Menge bis Priorität��5~   �    6�   "   Frank_UI.CultureAwareBinding  $ Menge bis Priorität�$
 {0:}��  5      $$ dgrLocation_FilterOptionsClick��6G   $# dgrLocation_DeleteFilterClick��6{   -3   5        dgrLocation�� . $ Fertigungsstelle��5   �    6�     $ Location�  5�      $' !dgrMitarbeiter_FilterOptionsClick��6�   $' !dgrMitarbeiter_FilterDeletedClick��6�   -4   5        dgrMitarbeiter�� . $ Mitarbeiter��5�   ^    6t   �� $ Mitarbeiter�6�     5�      $$ dgrVendorNo_FilterOptionsClick��6G   $# dgrVendorNo_DeleteFilterClick��6{   -5   5        dgrVendorNo�� . $ Kreditor Nr.��5�   �   $ 
Vendor No.��6�    6�   �� $ [Vendor No.]�  5�      $&  dgrVendorName_FilterOptionsClick��6I   $% dgrVendorName_DeleteFilterClick��6   -6   5        dgrVendorName�� . $ Kreditor Name��5�   �    6�   �� $ Vendor Name�  5�      -7   5        dgrQuantityAbleToProduce��$ Fertigen��5�   T    6g     $ Quantity able to Produce�  5�      -8   5        dgrMissingQuantity��$ Fehlende Menge��5�   N    6g     $ Missing Quantity�  5�      -9   5        dgrRecProdQuantity��$ Empf. Fertigungsmenge��5�   N   $  Recommended Prod. Quantity��6n    6�     $" [Recommended Prod. Quantity]�  5�      -:   5        dgrMinutenProStueck��$ Minuten pro Stück��5�   O    6k     $ Minuten pro Stück�  5�      -;   5        dgrMinutenFehlendeMenge��$ Minuten Fehlende Menge��5�   S    6t     $ Minuten Fehlende Menge�  5�      -<   5         dgrMinutenProduzierbareMenge��$! Minuten Produzierbare Menge��5�   X    6~     $! Minuten Produzierbare Menge�  5�      -=   5        dgrEuroProStueck��$ Euro Pro Stück��5�   L    6e     $ Euro pro Stück�  5�      ->   5        $ dgrEuroTatsaechlichFehlendeMenge��$&  Euro Tatsächlich Fehlende Menge��5�   \    6�     $&  Euro Tatsächlich Fehlende Menge�5�      5�      5�      �� 5�      $��Verkaufsaufträge��6   .��5�      � ���	#FFE5E5E5.+�5�          Frank_UI.mDataGridTreeView  -?   5          Name$
 dgtv��5�   �    	   Frank_UI.mDataGridTreeView 	  TreeViewItem_Expanded$ trvItem_Expanded��6,   ( 	  !TreeViewItem_MouseRightButtonDown$, &dgtv_TreeViewItem_MouseRightButtonDown��6U    	  TreeViewItem_Collapsed$ trvItem_Collapsed��6�   $��1��6�   $��0q�6�      Columns 5�        5�      $' !clmDescription_FilterOptionsClick��6�   $' !clmDescription_FilterDeletedClick��6�      DisplayIndex$ 3��62      Width5
  /System.Windows.Controls.DataGridLengthConverter$ 6*
 6C   $ Beschreibung��6N       CellType$  TreeViewItemText��6e    . 6  4  .System.Windows.Controls.DataGridTemplateColumn  5�      $ [Available]��65   $ 0��6R   $ Verfügbarkeit��6c   !   CellTemplate! 5�   "   �� 5�   &   .R�5�   *   �� ��6;   �� *[Available]+  BoolToCheckBox" �� 	Converter#" [ 5�   '   5�        5�      $$ clmCustomer_FilterOptionsClick��6r   $$ clmCustomer_FilterDeletedClick��6�   $ 2��62   $ 3*
 6C   $ Kunde��6N    . 6�     5�      $  clmDate_FilterOptionsClick��6�   $  clmDate_FilterDeletedClick��6�   $ 1��62   $ *
 6C   $ Liefertermin frühestens��6M    . 6�   5�      5�      5�      5�      �� 5�   
   -@   5        # ��Name$# groupBox1_Copy��5�   T   $��Aktionen��6   $
��Left=�6%   $��
311,76,0,7q�6@   $��Stretch=�6l   $	��209��6�   $��2��6�   .��5�      �� -A   5        $��	lblAction��5�      $�� ��6%   $	��Top=�60   $��6q�6H   5�      5�      5�      