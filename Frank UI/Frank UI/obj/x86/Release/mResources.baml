   M S B A M L   `   `   `  ���� C  ?Frank UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:Frank_UIFrank_UI      Frank_UI.mResourcesR NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089%   Frank_UI.StringToColorConverter   StringToColor   Frank_UI.BoolToCheckBox  BoolToCheckBox  
GlyphBrush  �� Color  Contrast  	Highlight  DataGridTreeViewStyle �� 
TargetType   Frank_UI.mTreeViewItem �� Property �� Value  	BaseStyle*  $System.Windows.Controls.DataGridCell �� 
TargetType ��Name �� RelativeSource �� Triggers P� Property	 P� Value  TreeViewItemContentStyle
 �� BasedOn   Frank_UI.mTreeViewItem   Expanded   	Collapsed
 � Tag �� AncestorType    Frank_UI.mDataGridTreeView  TreeViewItemTextStyle 	 ExpandCollapseToggleStyle F�Name �� Triggers �� 
TargetName 
 TreeViewItemFocusVisual �� StrokeDashArray J� Opacity  mTreeViewItemStyle �� Path � ColumnDefinitions � RowDefinitions d�Name �� 	ClickMode d� 	IsChecked �� Padding ��Name ]� 
Conditions �� Property �� Value  DeleteButton A�Name	  �� X1	! �� Y1	" �� X2	# �� Y2   D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     locclr-namespace:Frank_UI  %�  &	        &	      &	 
     &	      &	 -     &	 B     &	 e     &	 2    7  7  &	 �    7  7  &		 �    7  &	
 '    &	 �    7 
 7 	 &	 }	        �� $
  #444���� $  #DDEEFF���� $  	#FF667788���� "  .���  � ��$ 20���� "  .���  � �� �� "  .R��� #��z��#��z��#��z��._��� $ 	presenter������ *	Content+ �� *TemplatedParent+ P�  �@ 
IsSelected	 . . ���  � ��
 �   ��� "  8
   .���  � �� �� .R�R� $��0q�#��z��@�   treeviewitem_expanded treeviewitem_collapsed8�� ���� *[Key]+���� *	[Items]+ ��  �� "  �� "  8
   .���  � �� �� .R�R� #��z��#��z��#��z��@�   treeviewitem_expanded treeviewitem_collapsed8�� ���� *	Content+ �� *TemplatedParent+���� *	[Items]+ ��  �� "  �� " d�.���  � |� .  ��  � �� �� " d�.R�� -   $��15��$��13��
������  ��  �� "  .+�F� $ 
ExpandPath��$
��Left=�$��Center=�$��1,1,1,1q�8��  "���� 	=     � z  	= � 	=  z 	 P�  �@d�	IsChecked$
	 TrueY�. ��� $ 
ExpandPath�� � ��" ��     	= � z  	= � 	=  z 	�� .���  � �� �� .R��� ._��� $��0,0,0,0q�$��5��
���   �$	 1 2R�$ 0Q��� "  .���  � ��
 ���� ��  � �� �� $  HorizontalContentAlignment� �� " ����  � �� �� $ VerticalContentAlignment� �� " ����  � ��$ 1,0,0,0q���  � ��# � ����  �@�FocusVisualStyle8   ��  � �� �� "  .R��  �� $��19��$
��Auto ��� $
��Auto ��� $
��Auto ��� $��* � �� $
��Auto ��� .+�d� $ Expander��8�� $ Press=� �� $ 
IsExpanded� �� *TemplatedParent+�� $ 	PART_Icon��$��1��$
��Icon���� $��Bd��$��2��#��z��#��z��#��z��# z��._��� $ PART_Header��$��Header��#��z���� $ 	ItemsHost��$��1��$��2��$��2�� P�  �@Q�
IsExpanded	 .  . ��� $ 	ItemsHost�� � y�$ 	Collapsed=�P�  �@��HasItems	 .  . ��� $ Expander�� � y�$ Hidden=�]�  ��  � �� .  ��  � ��$
 Auto��./��� $ PART_Header�� � ��$ 75��]�  ��  � �� .  ��  � ��$
 Auto��./��� $ PART_Header�� � ��$ 19��P�  �@Q�
IsSelected	 . . ��� $ Bd��# � �� � ���� # � �� � ��]�  ��  �@Q�
IsSelected . ��  �@Q�IsSelectionActive .  ./��� $ Bd��# � �� � ���� # � �� � ��P�  � {�	 .  . ��� # � �� � ���� " ��.���  � ��$ 26����  � ��$ 26����  � ��$
 5, 0q���  � �� �� " ��.R��� .+�A� $ elMain��$��26��$��26��
���   �$	��0.5��
�������A� $��20��$��20��$��Center=�$��Center=�$��3q�
���   �$	��0.5���� $  8��$! 8��$" 18��$# 18��$	��0.5��
���   ��� $  8��$! 18��$" 18��$# 8��$	��0.5��
���   � P�  �@J�IsMouseOver	 . . ��� $ elMain�� � ��
 �����