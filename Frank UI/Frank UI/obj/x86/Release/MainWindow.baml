   M S B A M L   `   `   `  ���� C  ?Frank UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:Frank_UIFrank_UI      Frank_UI.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     &locclr-namespace:Frank_UI     -     � Title$  
Dispo-Tool��$
��1195��$��1612.717�� � WindowState$ 	Maximized=� � 	Resources  �� Source�� $ mResources.xaml?�%    .���  � RowDefinitions �� $��30 ��� $
��233* ��� $	��150��$	��300 ��� $��24 �.+��� -   $��gspHorizontal��$��5��$��Stretch=�$	��Top=� �� ResizeDirection$
 Auto=�$��2���� -   $��
statusBar1��$��3��$��26��$��Stretch=�$��Bottom=�.@���  �� Orientation$ 
Horizontal=�$��Stretch=�$
��5, 0q�.+��� .��Verkaufsaufträge:�� -   $��lblNumberOfSales��$��
0, 0, 5, 0q�.��-�� $ 
Horizontal=�$��Stretch=�$
��5, 0q�.+��� .��
Artikel:�� -   $��lblNumberofItems��$��
0, 0, 5, 0q�.��-�� $ 
Horizontal=�$��Stretch=�$
��5, 0q�.+��� .��Letzte Aktualisierung:�� -   $��lblLastUpdate��$��
0, 0, 5, 0q�.��-�� $ 
Horizontal=�$��Stretch=�$
��5, 0q�.+��� .��Minuten Fehlende Menge:�� -   $"��lblSumMinutesMissingQuantity��$��
0, 0, 5, 0q�.��-�� $ 
Horizontal=�$��Stretch=�$
��5, 0q�.+��� .��Minuten Produzierbare Menge:�� -   $%��lblSumMinutesProducableQuantity��$��
0, 0, 5, 0q�.��-�� $ 
Horizontal=�$��Stretch=�$
��5, 0q�.+��� .��Euro Aktuell Fehlende Menge:�� -	   $'��!lblSumEuroActuallyMissingQuantity��$��
0, 0, 5, 0q�.��-� $��2��$��
0, 5, 0, 0q�$��Stretch=�$��Stretch=�   Contrast#��[  .+��� -
   $��lblInfo��$��Informationen��$��28��$��Stretch=�$	��Top=�$��Center=��� -   $��lblArtikelnr��$"��Artikelnummer + Beschreibung��$��28��$
��Left=�$��6,34,0,0q�$	��Top=�$��15(�$
��Bold"��� -   $��label9��$-��'Wird in folgenden Aufträgen benötigt:��$��28��$
��Left=�$��
501,65,0,0q�$	��Top=�&   System.Windows.Controls.DataGrid  -   $��dgrdInSales��   
IsReadOnly .    AlternatingRowBackground# [  	   AutoGenerateColumns	 .  $��Stretch=�$��501,93,544,10q�$��Stretch=�
   GridLinesVisibility$

 None=�   CanUserResizeRows .     Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Menge��1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Menge+  $	 Nr.�� �� *Nr+  $ 
Priorität��   SortMemberPath$ PrioritätZahl��   SortDirection$ 
DescendingX� �� *
Priorität+  $ Liefertermin Bestätigt�� �� *ShipmentDate+ �� StringFormat$ {0:dd.MM.yy}��  $ Kunde�� �� *Kunde+  $ Artikel�� �� *	Artikel+  -   $��dgrdRequisition�� . # [  $

 None=� .  	 .  $��Right=�$��0,93,379,10q�$��Stretch=�$	��160��   $ Erstellt Am�� �� *Erstellt Am+$ {0:dd.MM.yy}��  $ 	Lieferant��$ 
Vendor No.�� ��  �� Path$ [Vendor No.]��� -    ��Name$ lblRequisition��$��Materialanforderungen:��$��28��$��Right=�$��
0,65,404,0q�$	��Top=�  -     Name$ dgrdPurchase�� . # [  $

 None=� .  	 .  $��Right=�$��
0,93,10,10q�$��Stretch=�$	��364��   $ Erwarteter Eingang�� �� *Expected Receipt Date+$ {0:dd.MM.yy}��  $ 	Lieferant��$ 
Vendor No.�� `� $ 	{0} - {1}��.1��� $ [Vendor No.]��� $ Vendor Name�  $ 
Bestellung��$ 
Vendor No.�� �� *[Purchase No.]+�� -   $ lblPurchase��$��Bestellungen:��$��28��$��Right=�$��
0,65,293,0q�$	��Top=��� -   $��	groupBox1��$��	Allgemein��$
��Left=�$��6,73,0,7q�$��Stretch=�$	��300��$��2��.��� .+��� -   $��lv��$��Stretch=�$��Stretch=�����  �� 
TargetType" ��.���  �� Property � �� �� Value$ Stretch=� �� View �� ! �� ColumnHeaderContainerStyle �� " ��.���  � y�$ 	Collapsed=�.P���  �� Width$	 160��$
��Name�� �� DisplayMemberBinding �� *Name+�� $	 120��$
��Wert�� �� *Wert+�� -   $��label11��$y�Hidden=�$��Suchen:��$��1��$��28��$
��Left=�$	��Top=�$��0,3,0,0q�o� -   $��menu1��$��23��$��Stretch=�$	��Top=�.@�m� $��B_earbeiten��.@�m� -   $��mitmUpdateData��$��Daten aktualisieren��m� -   $��mitmResetFilters��$��Alle Filter zurücksetzen��m� $��_Hilfe��.@�m� -   $��mtmAbout��$��Über Dispo-Tool��m� -   $��	mtmManual��$��Handbuch��m� $��Entwickler-Tools��.@�m� -   $��mtmAdvancedFilter��$��Advanced Filter���� $��Stretch=�$��0,0,0,0q�$��1��$��Stretch=�.@��� $��Artikel��.��� ���	#FFE5E5E5.+�  -   $	��dgr��$y�Visible=�# [   . $

 None=�$��1��$��0q�	 .     Frank_UI.mDataGridColumn    Frank_UI.mDataGridColumn   FilterOptionsClick$" dgrAction_FilterOptionsClick��   FilterDeletedClick$! dgrAction_DeleteFilterClick��-   	dgrAction��   EnableFilter .    Caption$ Aktion�� �� $ Action�  $) #dgrReplenishment_FilterOptionsClick��$( "dgrReplenishment_DeleteFilterClick��-   dgrReplenishment�� . $ Beschaffungsmethode�� �� $ Replenishment System�  $" dgrItemNo_FilterOptionsClick��$! dgrItemNo_DeleteFilterClick��-   	dgrItemNo�� . $ Artikelnummer��$	 No.�� �� $ [No.]�  $' !dgrDescription_FilterOptionsClick��$&  dgrDescription_DeleteFilterClick��-   dgrDescription�� . $ Beschreibung�� �� $ Description�  $( "dgrNextPriority_FilterOptionsClick��$' !dgrNextPriority_DeleteFilterClick��-    dgrNextPriority��$ Nächste Priorität Zahl�� .  $ Nächste Priorität�� `� $ {0}{1}��.1��� $$ Nächste Priorität Buchstaben��� $ Nächste Priorität Zahl�  -!   dgrShipmentDate��$' !Nächster Liefertermin bestätigt��    TextType$
  Date=� �� $' !Nächster Liefertermin bestätigt�$ {0:dd.MM.yy}��  $$ dgrLocation_FilterOptionsClick��$# dgrLocation_DeleteFilterClick��-"   dgrLocation�� . $ Fertigungsstelle�� "   Frank_UI.CultureAwareBinding  $ Location�  $' !dgrMitarbeiter_FilterOptionsClick��$' !dgrMitarbeiter_FilterDeletedClick��-#   dgrMitarbeiter�� . $ Mitarbeiter�� �� $ Mitarbeiter�  $$ dgrVendorNo_FilterOptionsClick��$# dgrVendorNo_DeleteFilterClick��-$   dgrVendorNo�� . $ Kreditor Nr.��$ 
Vendor No.�� �� $ [Vendor No.]�  $&  dgrVendorName_FilterOptionsClick��$% dgrVendorName_DeleteFilterClick��-%   dgrVendorName�� . $ Kreditor Name�� �� $ Vendor Name�  -&   dgrQuantityAbleToProduce��$  Number=�$ Fertigen��   $ Quantity able to Produce�  -'   dgrMissingQuantity��$  Number=�$ Fehlende Menge��   $ Missing Quantity�  -(   dgrRecProdQuantity��$  Number=�$ Empf. Fertigungsmenge��$  Recommended Prod. Quantity��   $" [Recommended Prod. Quantity]�  -)   dgrMinutenProStueck��$  Number=�$ Minuten pro Stück��   $ Minuten pro Stück�  -*   dgrMinutenFehlendeMenge��$  Number=�$ Minuten Fehlende Menge��   $ Minuten Fehlende Menge�  -+    dgrMinutenProduzierbareMenge��$  Number=�$! Minuten Produzierbare Menge��   $! Minuten Produzierbare Menge�  -,   dgrEuroProStueck��$  Currency=�$ Euro Pro Stück��   $ Euro pro Stück�$ c��  --   $ dgrEuroTatsaechlichFehlendeMenge��$  Currency=�$! Euro Aktuell Fehlende Menge��   $&  Euro Tatsächlich Fehlende Menge�$ c��!   ElementStyle! �� " ��.���  � ��$ Right=��� $��Verkaufsaufträge��.��� ���	#FFE5E5E5.+�    Frank_UI.mDataGridTreeView  -.   "  Name$
" dgtv�� 	   Frank_UI.mDataGridTreeView# 	  TreeViewItem_Expanded$# trvItem_Expanded��($ 	  !TreeViewItem_MouseRightButtonDown$,$ &dgtv_TreeViewItem_MouseRightButtonDown��% 	  TreeViewItem_Collapsed$% trvItem_Collapsed��& 	  LayoutUpdated$& dgtv_LayoutUpdated��$��1��$��0q�'   Columns'   $' !clmDescription_FilterOptionsClick��$' !clmDescription_FilterDeletedClick��(   DisplayIndex$( 5��)   Width5
  /System.Windows.Controls.DataGridLengthConverter$) 6*
 $ Beschreibung��*   CellType$* TreeViewItemText�� . 4  .System.Windows.Controls.DataGridTemplateColumn  $ [Available]��$( 0��$ Verfügbarkeit��+   CellTemplate+ �� .R��� ���� *[Available]+  BoolToCheckBox, �� 	Converter#, [   $$ clmCustomer_FilterOptionsClick��$$ clmCustomer_FilterDeletedClick��$( 2��$) 3*
 $ Kunde�� .   $  clmDate_FilterOptionsClick��$  clmDate_FilterDeletedClick��$
  Date=�$( 1��$) *
 $ Liefertermin bestätigt�� .   $  Number=�$( 3��$) *
 $ Minuten Positionsmenge��  $  Currency=�$( 4��$) *
 $ Euro Positionsmenge��  $  Number=�$( 6��$) *
 $ 
Priorität���� -/   - ��Name$- groupBox1_Copy��$��Aktionen��$
��Left=�$��311,75.667,0,7.333q�$��Stretch=�$	��182��$��2��.���� -0   $��	lblAction��$�� ��$	��Top=�$��6q�