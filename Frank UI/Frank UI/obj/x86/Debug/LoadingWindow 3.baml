   M S B A M L   `   `   `  ����C  ?Frank UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:Frank_UIFrank_UI  5   	       Frank_UI.LoadingWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:Frank_UI     5   	     � Title$  LoadingWindow��5   	   $	��300��6   $	��600��6,    � WindowStartupLocation$ CenterScreen=�68    � 
ResizeMode$ NoResize=�6]    � WindowStyle$
 None=�6s   .��5	      � .+�5
   
   �� -   5        $��imglogo��5
      $
��Left=�6   $	��256��6:   $��Center=�6G   $	��256��6b   $+��%/Resources/android-chrome-256x256.png��6n   $��
15,22,0,22q�6�   �� 5   
   $��Frank Dispo-tool��6   $��Right=�6+   $��0,7,64,0q�6G   $	��Top=�6Y   $��Yu Gothic UI Light)�6q   $��36(�6�   ���	#FFC1C1C16�   �� 5   
   $��Lade Daten...��6   $
��Left=�6(   $��
278,94,0,0q�6C   $	��Top=�6W   �� 5   
   -   5        $��lblCopyright��5      $��© Samir El-Assal jr yyyy��6$   $
��Left=�6G   $��278,252,0,0q�6b   $	��Top=�6w   � 5   
   -   5        $��grdRotation��5      $��75��6"   $��76��6.   $��293,147,231,0q�69   $��Center=�6P   $	��Top=�6m    J� RenderTransformOrigin$ 0.5, 0.5&�6�   z�5      �� 5      -   5        rotation�� �� Angle$ 0Q�5   4    � Triggers 5      :� 5       :� RoutedEvent$ Loaded��6   .[�5      �� .��5      �� .��5      W�  �� TargetProperty$4 .(Grid.RenderTransform).(RotateTransform.Angle)�6.   		 W� To$		 360X�6y   
 g� Duration$
 0:0:1D�6�    g� RepeatBehavior$ Forever��6�   5      5      5      .+�5      A� $��Center=�6   $��75��63   $��Center=�6?   $��75��6Z   $��1��6e   
���   �6y   �� 5      $��Center=�6   $��14��65   $��Center=�6A   $��20��6\   
�������$��28,62,27,-1q�6t   5      5      5       