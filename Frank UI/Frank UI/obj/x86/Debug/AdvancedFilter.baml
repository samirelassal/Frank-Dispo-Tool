   M S B A M L   `   `   `  ����C  ?Frank UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:Frank_UIFrank_UI  5   	       Frank_UI.AdvancedFilter   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:Frank_UI     5   	     � Title$  AdvancedFilter��5   	    � WindowState$ 	Maximized=�6    .��5	      � .+�5
   
   �� $��Filter String:��6   $
��Left=�6)   $��	10,10,0,0q�6D   $	��Top=�6W   $��80��6o   �� 5   
   -   5        $��txtFilterString��5      $��Stretch=�6)   $��23��6G   $��
90,14,90,0q�6S    �� TextWrapping$
 Wrap=�6g   $�� ��6{   $	��Top=�6�   �� 5   
   -   5        $��cmbDataTable��5      $
��Left=�6'   $��	90,42,0,0q�6B   $	��Top=�6U   $	��180��6m   @�5      �� 5      .��6   Calculated_Item60   �� 5      .��6   Calculated_Sale60   �� 5      .��6   Calculated_Requisition67   �� 5      .��6   Calculated_Purchase64   �� 5      .��6   SalesHeader6,   5      �� 5   
   $��
DataTable:��6   $
��Left=�6%   $��	10,42,0,0q�6@   $	��Top=�6S   $��75��6k   �� 5   
   -   5        $��btnGo��5      $	��Go!��6   $��Right=�6,   $��	0,14,10,0q�6H   $	��Top=�6[   $��75��6s   $��23��6~   6�   �� 5   
   $��Stretch=�6   $��0,70,0,0q�64   $��Stretch=�6F   .@�5      �� $
��Data��6   .��5      � ���	#FFE5E5E5.+�5      &   System.Windows.Controls.DataGrid  -   5        $	��dgr��5      $��Stretch=�6*   $��Stretch=�6H   6d   5      5      �� 5      $
��Meta��6   .��5      � ���	#FFE5E5E56   5      5       5"      5#      