<?php
/*
  Plugin Name: KintoneQAPlugin
  Plugin URI:
  Description: Googleフォームをスクレイピングし、内容をKintoneフォームに変換・作成するツールです。
  Version: 1.0.0
  Author: tokuda
  Author URI: https://github.com/Polyrhythm-Inc/wp-plugin-kintone-poc
  License: GPLv2
 */

add_action('init', 'KintoneQA::init');

class KintoneQA
{
    // TODO 対象のURLに変更する
    private $url = "https://nextjsapp.com";

    public static function init()
    {
        return new self;
    }

    public function __construct()
    {
        if (is_admin() && is_user_logged_in()) {
            add_action('admin_menu', [$this, 'set_plugin_menu']);
        }
    }

    public function set_plugin_menu()
    {
        add_menu_page(
            'Kintone QA Admin',           /* ページタイトル*/
            'Kintone QA Admin',           /* メニュータイトル */
            'manage_options',         /* 権限 */
            'KintoneQAAdmin',    /* ページを開いたときのURL */
            [$this, 'show_iframe'],       /* メニューに紐づく画面を描画するcallback関数 */
            'dashicons-format-gallery', /* アイコン see: https://developer.wordpress.org/resource/dashicons/#awards */
            99                          /* 表示位置のオフセット */
        );
    }

    public function show_iframe()
    {
        echo "<iframe  width=\"700\" height=\"500\" src=\"" . $this -> url . "\"></iframe>";
    }
}
