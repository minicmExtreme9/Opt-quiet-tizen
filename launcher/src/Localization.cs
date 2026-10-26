using System;
using System.Collections.Generic;
using System.Globalization;

public static class Localization
{
    public static readonly string[] LanguageCodes = new[] { "es", "en", "de", "pt", "fr", "zh", "ja", "ko", "hi", "ar" };
    public static readonly string[] LanguageNames = new[] { "Español", "English", "Deutsch", "Português", "Français", "中文 (Mandarin)", "日本語", "한국어", "हिन्दी", "العربية" };

    public static readonly Dictionary<string, Dictionary<string, string>> Strings = new Dictionary<string, Dictionary<string, string>>
    {
        ["home"] = new Dictionary<string, string>
        {
            ["es"] = "Inicio",
            ["en"] = "Home",
            ["de"] = "Start",
            ["pt"] = "Início",
            ["fr"] = "Accueil",
            ["zh"] = "主页",
            ["ja"] = "ホーム",
            ["ko"] = "홈",
            ["hi"] = "होम",
            ["ar"] = "الرئيسية",
        },
        ["inputs"] = new Dictionary<string, string>
        {
            ["es"] = "Entradas",
            ["en"] = "Inputs",
            ["de"] = "Eingänge",
            ["pt"] = "Entradas",
            ["fr"] = "Entrées",
            ["zh"] = "输入",
            ["ja"] = "入力",
            ["ko"] = "입력",
            ["hi"] = "इनपुट",
            ["ar"] = "المداخل",
        },
        ["apps"] = new Dictionary<string, string>
        {
            ["es"] = "Apps",
            ["en"] = "Apps",
            ["de"] = "Apps",
            ["pt"] = "Apps",
            ["fr"] = "Apps",
            ["zh"] = "应用",
            ["ja"] = "アプリ",
            ["ko"] = "앱",
            ["hi"] = "ऐप्स",
            ["ar"] = "التطبيقات",
        },
        ["settings"] = new Dictionary<string, string>
        {
            ["es"] = "Ajustes TV",
            ["en"] = "TV Settings",
            ["de"] = "TV-Einstellungen",
            ["pt"] = "Configurações TV",
            ["fr"] = "Paramètres TV",
            ["zh"] = "电视设置",
            ["ja"] = "テレビ設定",
            ["ko"] = "TV 설정",
            ["hi"] = "टीवी सेटिंग्स",
            ["ar"] = "إعدادات التلفاز",
        },
        ["quick_settings"] = new Dictionary<string, string>
        {
            ["es"] = "Ajustes Rápidos",
            ["en"] = "Quick Settings",
            ["de"] = "Schnelleinstellungen",
            ["pt"] = "Configurações Rápidas",
            ["fr"] = "Paramètres Rapides",
            ["zh"] = "快捷设置",
            ["ja"] = "クイック設定",
            ["ko"] = "빠른 설정",
            ["hi"] = "त्वरित सेटिंग्स",
            ["ar"] = "الإعدادات السريعة",
        },
        ["your_apps"] = new Dictionary<string, string>
        {
            ["es"] = "Tus Aplicaciones",
            ["en"] = "Your Apps",
            ["de"] = "Ihre Apps",
            ["pt"] = "Seus Aplicativos",
            ["fr"] = "Vos Applications",
            ["zh"] = "你的应用",
            ["ja"] = "マイアプリ",
            ["ko"] = "내 앱",
            ["hi"] = "आपके ऐप्स",
            ["ar"] = "تطبيقاتك",
        },
        ["launcher_settings"] = new Dictionary<string, string>
        {
            ["es"] = "Preferencias del launcher",
            ["en"] = "Launcher Preferences",
            ["de"] = "Launcher-Einstellungen",
            ["pt"] = "Preferências do launcher",
            ["fr"] = "Préférences du lanceur",
            ["zh"] = "启动器偏好设置",
            ["ja"] = "ランチャー設定",
            ["ko"] = "런처 환경설정",
            ["hi"] = "लॉन्चर प्राथमिकताएं",
            ["ar"] = "تفضيلات المشغل",
        },
        ["launcher_settings_desc"] = new Dictionary<string, string>
        {
            ["es"] = "Ajustes · Nombres HDMI, idioma y sonido",
            ["en"] = "Settings · HDMI names, language & sound",
            ["de"] = "Einstellungen · HDMI-Namen, Sprache & Ton",
            ["pt"] = "Ajustes · Nomes HDMI, idioma e som",
            ["fr"] = "Paramètres · Noms HDMI, langue et son",
            ["zh"] = "设置 · HDMI名称、语言与声音",
            ["ja"] = "設定 · HDMI名、言語、サウンド",
            ["ko"] = "설정 · HDMI 이름, 언어 및 사운드",
            ["hi"] = "सेटिंग्स · HDMI नाम, भाषा और ध्वनि",
            ["ar"] = "الإعدادات · أسماء HDMI واللغة والصوت",
        },
        ["back_to_input"] = new Dictionary<string, string>
        {
            ["es"] = "Volver a entrada",
            ["en"] = "Back to input",
            ["de"] = "Zurück zum Eingang",
            ["pt"] = "Voltar para entrada",
            ["fr"] = "Retour à la source",
            ["zh"] = "返回输入",
            ["ja"] = "入力に戻る",
            ["ko"] = "입력으로 돌아가기",
            ["hi"] = "इनपुट पर वापस जाएं",
            ["ar"] = "العودة إلى المدخل",
        },
        ["back_to_home"] = new Dictionary<string, string>
        {
            ["es"] = "Volver al Inicio",
            ["en"] = "Back to Home",
            ["de"] = "Zurück zum Start",
            ["pt"] = "Voltar ao Início",
            ["fr"] = "Retour à l'accueil",
            ["zh"] = "返回主页",
            ["ja"] = "ホームに戻る",
            ["ko"] = "홈으로 돌아가기",
            ["hi"] = "होम पर वापस जाएं",
            ["ar"] = "العودة إلى الرئيسية",
        },
        ["reorder_hint"] = new Dictionary<string, string>
        {
            ["es"] = "Flechas: mover · OK: fijar · Back: salir",
            ["en"] = "Arrows: move · OK: set · Back: exit",
            ["de"] = "Pfeile: bewegen · OK: fixieren · Zurück: beenden",
            ["pt"] = "Setas: mover · OK: fixar · Voltar: sair",
            ["fr"] = "Flèches: déplacer · OK: fixer · Retour: quitter",
            ["zh"] = "方向键: 移动 · OK: 确定 · Back: 退出",
            ["ja"] = "矢印: 移動 · OK: 決定 · Back: 終了",
            ["ko"] = "화살표: 이동 · OK: 고정 · Back: 나가기",
            ["hi"] = "तीर: ले जाएं · ठीक: सेट · वापस: बाहर",
            ["ar"] = "الأسهم: تحريك · موافق: تثبيت · رجوع: خروج",
        },
        ["sound"] = new Dictionary<string, string>
        {
            ["es"] = "Sonido",
            ["en"] = "Sound",
            ["de"] = "Ton",
            ["pt"] = "Som",
            ["fr"] = "Son",
            ["zh"] = "声音",
            ["ja"] = "サウンド",
            ["ko"] = "소리",
            ["hi"] = "ध्वनि",
            ["ar"] = "الصوت",
        },
        ["sound_on"] = new Dictionary<string, string>
        {
            ["es"] = "Activado",
            ["en"] = "Enabled",
            ["de"] = "Aktiviert",
            ["pt"] = "Ativado",
            ["fr"] = "Activé",
            ["zh"] = "已开启",
            ["ja"] = "オン",
            ["ko"] = "켜짐",
            ["hi"] = "चालू",
            ["ar"] = "مفعل",
        },
        ["sound_off"] = new Dictionary<string, string>
        {
            ["es"] = "Desactivado",
            ["en"] = "Disabled",
            ["de"] = "Deaktiviert",
            ["pt"] = "Desativado",
            ["fr"] = "Désactivé",
            ["zh"] = "已关闭",
            ["ja"] = "オフ",
            ["ko"] = "꺼짐",
            ["hi"] = "बंद",
            ["ar"] = "معطل",
        },
        ["animations"] = new Dictionary<string, string>
        {
            ["es"] = "Animaciones",
            ["en"] = "Animations",
            ["de"] = "Animationen",
            ["pt"] = "Animações",
            ["fr"] = "Animations",
            ["zh"] = "动效",
            ["ja"] = "アニメーション",
            ["ko"] = "애니메이션",
            ["hi"] = "एनिमेशन",
            ["ar"] = "الرسوم المتحركة",
        },
        ["language"] = new Dictionary<string, string>
        {
            ["es"] = "Idioma",
            ["en"] = "Language",
            ["de"] = "Sprache",
            ["pt"] = "Idioma",
            ["fr"] = "Langue",
            ["zh"] = "语言",
            ["ja"] = "言語",
            ["ko"] = "언어",
            ["hi"] = "भाषा",
            ["ar"] = "اللغة",
        },
        ["save_close"] = new Dictionary<string, string>
        {
            ["es"] = "Guardar y Cerrar (OK)",
            ["en"] = "Save & Close (OK)",
            ["de"] = "Speichern & Schließen (OK)",
            ["pt"] = "Salvar e Fechar (OK)",
            ["fr"] = "Enregistrer et Fermer (OK)",
            ["zh"] = "保存并关闭 (OK)",
            ["ja"] = "保存して閉じる (OK)",
            ["ko"] = "저장 및 닫기 (OK)",
            ["hi"] = "सहेजें और बंद करें (OK)",
            ["ar"] = "حفظ وإغلاق (OK)",
        },
        ["switch_to"] = new Dictionary<string, string>
        {
            ["es"] = "Cambiar a",
            ["en"] = "Switch to",
            ["de"] = "Wechseln zu",
            ["pt"] = "Mudar para",
            ["fr"] = "Basculer vers",
            ["zh"] = "切换至",
            ["ja"] = "切り替え:",
            ["ko"] = "전환:",
            ["hi"] = "बदलें:",
            ["ar"] = "التبديل إلى",
        },
        ["lets_play"] = new Dictionary<string, string>
        {
            ["es"] = "A jugar.",
            ["en"] = "Let's play.",
            ["de"] = "Lass uns spielen.",
            ["pt"] = "Vamos jogar.",
            ["fr"] = "C'est parti.",
            ["zh"] = "开始游玩。",
            ["ja"] = "プレイしよう。",
            ["ko"] = "플레이하기.",
            ["hi"] = "खेल शुरू करें।",
            ["ar"] = "لنلعب.",
        },
        ["volume"] = new Dictionary<string, string>
        {
            ["es"] = "Volumen",
            ["en"] = "Volume",
            ["de"] = "Lautstärke",
            ["pt"] = "Volume",
            ["fr"] = "Volume",
            ["zh"] = "音量",
            ["ja"] = "音量",
            ["ko"] = "음량",
            ["hi"] = "आवाज़",
            ["ar"] = "مستوى الصوت",
        },
        ["brightness"] = new Dictionary<string, string>
        {
            ["es"] = "Brillo",
            ["en"] = "Brightness",
            ["de"] = "Helligkeit",
            ["pt"] = "Brilho",
            ["fr"] = "Luminosité",
            ["zh"] = "亮度",
            ["ja"] = "明るさ",
            ["ko"] = "밝기",
            ["hi"] = "चमक",
            ["ar"] = "السطوع",
        },
        ["game_mode"] = new Dictionary<string, string>
        {
            ["es"] = "Modo Juego",
            ["en"] = "Game Mode",
            ["de"] = "Spielmodus",
            ["pt"] = "Modo Jogo",
            ["fr"] = "Mode Jeu",
            ["zh"] = "游戏模式",
            ["ja"] = "ゲームモード",
            ["ko"] = "게임 모드",
            ["hi"] = "गेम मोड",
            ["ar"] = "وضع الألعاب",
        },
        ["sleep_timer"] = new Dictionary<string, string>
        {
            ["es"] = "Temporizador",
            ["en"] = "Sleep Timer",
            ["de"] = "Schlummerfunktion",
            ["pt"] = "Temporizador",
            ["fr"] = "Minuteur",
            ["zh"] = "睡眠定时器",
            ["ja"] = "スリープタイマー",
            ["ko"] = "취침 예약",
            ["hi"] = "स्लीप टाइमर",
            ["ar"] = "مؤقت النوم",
        },
        ["controllers"] = new Dictionary<string, string>
        {
            ["es"] = "Mandos",
            ["en"] = "Controllers",
            ["de"] = "Controller",
            ["pt"] = "Controles",
            ["fr"] = "Manettes",
            ["zh"] = "手柄控制器",
            ["ja"] = "コントローラー",
            ["ko"] = "컨트롤러",
            ["hi"] = "कंट्रोलर्स",
            ["ar"] = "أجهزة التحكم",
        },
        ["network"] = new Dictionary<string, string>
        {
            ["es"] = "Red",
            ["en"] = "Network",
            ["de"] = "Netzwerk",
            ["pt"] = "Rede",
            ["fr"] = "Réseau",
            ["zh"] = "网络",
            ["ja"] = "ネットワーク",
            ["ko"] = "네트워크",
            ["hi"] = "नेटवर्क",
            ["ar"] = "الشبكة",
        },
        ["advanced"] = new Dictionary<string, string>
        {
            ["es"] = "Ajustes Samsung",
            ["en"] = "Advanced",
            ["de"] = "Erweitert",
            ["pt"] = "Avançado",
            ["fr"] = "Avancé",
            ["zh"] = "三星设置",
            ["ja"] = "詳細設定",
            ["ko"] = "고급 설정",
            ["hi"] = "उन्नत सेटिंग्स",
            ["ar"] = "إعدادات متقدمة",
        },
        ["modal_hint"] = new Dictionary<string, string>
        {
            ["es"] = "Flechas para cambiar | Botón de atrás o \\\"OK\\\" para guardar",
            ["en"] = "Arrows to change | Back button or \\\"OK\\\" to save",
            ["de"] = "Pfeile zum Ändern | Zurück-Taste oder „OK“ zum Speichern",
            ["pt"] = "Setas para mudar | Botão Voltar ou \\\"OK\\\" para salvar",
            ["fr"] = "Flèches pour changer | Bouton Retour ou « OK » pour enregistrer",
            ["zh"] = "方向键更改 | 返回键或 \\\"OK\\\" 保存",
            ["ja"] = "矢印キーで変更 | 戻るまたは「OK」で保存",
            ["ko"] = "화살표로 변경 | 뒤로가기 또는 \\\"OK\\\"로 저장",
            ["hi"] = "बदलने के लिए तीर | सहेजने के लिए वापस या \\\"OK\\\"",
            ["ar"] = "الأسهم للتغيير | زر الرجوع أو \\\"OK\\\" للحفظ",
        },
        ["volume_desc"] = new Dictionary<string, string>
        {
            ["es"] = "Seleccionar · Flechas para ajustar",
            ["en"] = "Select · Left / Right to adjust",
            ["de"] = "Auswählen · Links / Rechts anpassen",
            ["pt"] = "Selecionar · Esquerda / Direita ajustar",
            ["fr"] = "Sélectionner · Gauche / Droite ajuster",
            ["zh"] = "选择 · 左右键调节",
            ["ja"] = "選択 · 左右で調整",
            ["ko"] = "선택 · 좌우로 조절",
            ["hi"] = "चुनें · समायोजित करने के लिए बाएं/दाएं",
            ["ar"] = "اختر · يمين / يسار للضبط",
        },
        ["brightness_desc"] = new Dictionary<string, string>
        {
            ["es"] = "Seleccionar · Flechas para ajustar",
            ["en"] = "Select · Left / Right to adjust",
            ["de"] = "Auswählen · Links / Rechts anpassen",
            ["pt"] = "Selecionar · Esquerda / Direita ajustar",
            ["fr"] = "Sélectionner · Gauche / Droite ajuster",
            ["zh"] = "选择 · 左右键调节",
            ["ja"] = "選択 · 左右で調整",
            ["ko"] = "선택 · 좌우로 조절",
            ["hi"] = "चुनें · समायोजित करने के लिए बाएं/दाएं",
            ["ar"] = "اختر · يمين / يسار للضبط",
        },
        ["game_mode_desc"] = new Dictionary<string, string>
        {
            ["es"] = "Seleccionar para activar o desactivar",
            ["en"] = "Select to switch on or off",
            ["de"] = "Auswählen zum Ein- oder Ausschalten",
            ["pt"] = "Selecionar para ligar ou desligar",
            ["fr"] = "Sélectionner pour activer ou désactiver",
            ["zh"] = "选择以开启或关闭",
            ["ja"] = "選択してオン/オフ切り替え",
            ["ko"] = "선택하여 켜기 또는 끄기",
            ["hi"] = "चालू या बंद करने के लिए चुनें",
            ["ar"] = "اختر للتشغيل أو الإيقاف",
        },
        ["sleep_timer_desc"] = new Dictionary<string, string>
        {
            ["es"] = "Seleccionar para cambiar temporizador",
            ["en"] = "Select to change the timer",
            ["de"] = "Auswählen zum Ändern des Timers",
            ["pt"] = "Selecionar para alterar o temporizador",
            ["fr"] = "Sélectionner pour modifier le minuteur",
            ["zh"] = "选择以更改定时器",
            ["ja"] = "選択してタイマーを変更",
            ["ko"] = "선택하여 타이머 변경",
            ["hi"] = "टाइमर बदलने के लिए चुनें",
            ["ar"] = "اختر لتغيير المؤقت",
        },
        ["mute_desc"] = new Dictionary<string, string>
        {
            ["es"] = "Seleccionar para silenciar o activar",
            ["en"] = "Select to mute or unmute",
            ["de"] = "Auswählen zum Stummschalten",
            ["pt"] = "Selecionar para ativar ou desativar som",
            ["fr"] = "Sélectionner pour couper ou activer le son",
            ["zh"] = "选择以静音或开启声音",
            ["ja"] = "選択して消音または解除",
            ["ko"] = "선택하여 음소거 또는 해제",
            ["hi"] = "म्यूट या अनम्यूट करने के लिए चुनें",
            ["ar"] = "اختر للكتم أو إلغاء الكتم",
        },
        ["controllers_desc"] = new Dictionary<string, string>
        {
            ["es"] = "DualSense · mantén pulsado PS + Create",
            ["en"] = "DualSense · hold PS + Create",
            ["de"] = "DualSense · PS + Create gedrückt halten",
            ["pt"] = "DualSense · segure PS + Create",
            ["fr"] = "DualSense · maintenir PS + Create",
            ["zh"] = "DualSense · 按住 PS + Create 键",
            ["ja"] = "DualSense · PS + Create を長押し",
            ["ko"] = "DualSense · PS + 만들기 버튼 길게 누름",
            ["hi"] = "DualSense · PS + Create दबाकर रखें",
            ["ar"] = "DualSense · اضغط مطولاً على PS + Create",
        },
        ["network_desc"] = new Dictionary<string, string>
        {
            ["es"] = "Configuración de red y Wi-Fi",
            ["en"] = "Wi-Fi and connection setup",
            ["de"] = "WLAN- und Netzwerkeinrichtung",
            ["pt"] = "Configuração de Wi-Fi e rede",
            ["fr"] = "Configuration Wi-Fi et réseau",
            ["zh"] = "Wi-Fi 与网络设置",
            ["ja"] = "Wi-Fi およびネットワーク設定",
            ["ko"] = "Wi-Fi 및 네트워크 설정",
            ["hi"] = "Wi-Fi और नेटवर्क सेटअप",
            ["ar"] = "إعدادات Wi-Fi والشبكة",
        },
        ["advanced_desc"] = new Dictionary<string, string>
        {
            ["es"] = "Abrir ajustes completos de Samsung",
            ["en"] = "Open Samsung's full settings",
            ["de"] = "Vollständige Samsung-Einstellungen",
            ["pt"] = "Abrir configurações completas da Samsung",
            ["fr"] = "Ouvrir tous les paramètres Samsung",
            ["zh"] = "打开三星完整系统设置",
            ["ja"] = "Samsungの全設定を開く",
            ["ko"] = "Samsung 전체 설정 열기",
            ["hi"] = "सैमसंग की पूरी सेटिंग्स खोलें",
            ["ar"] = "فتح إعدادات سامسونج الكاملة",
        },
        ["pair_controller"] = new Dictionary<string, string>
        {
            ["es"] = "Vincular mando",
            ["en"] = "Pair controller",
            ["de"] = "Controller koppeln",
            ["pt"] = "Emparelhar controle",
            ["fr"] = "Jumeler la manette",
            ["zh"] = "配对手柄",
            ["ja"] = "コントローラー接続",
            ["ko"] = "컨트롤러 페어링",
            ["hi"] = "कंट्रोलर जोड़ें",
            ["ar"] = "إقران وحدة التحكم",
        },
        ["connections"] = new Dictionary<string, string>
        {
            ["es"] = "Conexiones",
            ["en"] = "Connections",
            ["de"] = "Verbindungen",
            ["pt"] = "Conexões",
            ["fr"] = "Connexions",
            ["zh"] = "网络连接",
            ["ja"] = "接続設定",
            ["ko"] = "연결",
            ["hi"] = "कनेक्शन",
            ["ar"] = "الاتصالات",
        },
        ["all_settings"] = new Dictionary<string, string>
        {
            ["es"] = "Todos los ajustes",
            ["en"] = "All settings",
            ["de"] = "Alle Einstellungen",
            ["pt"] = "Todas as configurações",
            ["fr"] = "Tous les paramètres",
            ["zh"] = "全部设置",
            ["ja"] = "すべての設定",
            ["ko"] = "모든 설정",
            ["hi"] = "सभी सेटिंग्स",
            ["ar"] = "كافة الإعدادات",
        },
        ["unavailable"] = new Dictionary<string, string>
        {
            ["es"] = "No disponible",
            ["en"] = "Unavailable",
            ["de"] = "Nicht verfügbar",
            ["pt"] = "Indisponível",
            ["fr"] = "Indisponible",
            ["zh"] = "不可用",
            ["ja"] = "利用不可",
            ["ko"] = "사용 불가",
            ["hi"] = "अनुपलब्ध",
            ["ar"] = "غير متاح",
        },
        ["on"] = new Dictionary<string, string>
        {
            ["es"] = "Activado",
            ["en"] = "On",
            ["de"] = "Ein",
            ["pt"] = "Ligado",
            ["fr"] = "Activé",
            ["zh"] = "开启",
            ["ja"] = "オン",
            ["ko"] = "켜짐",
            ["hi"] = "चालू",
            ["ar"] = "تشغيل",
        },
        ["off"] = new Dictionary<string, string>
        {
            ["es"] = "Desactivado",
            ["en"] = "Off",
            ["de"] = "Aus",
            ["pt"] = "Desligado",
            ["fr"] = "Désactivé",
            ["zh"] = "关闭",
            ["ja"] = "オフ",
            ["ko"] = "꺼짐",
            ["hi"] = "बंद",
            ["ar"] = "إيقاف",
        },
        ["muted"] = new Dictionary<string, string>
        {
            ["es"] = "Silenciado",
            ["en"] = "Muted",
            ["de"] = "Stumm",
            ["pt"] = "Mudo",
            ["fr"] = "Muet",
            ["zh"] = "已静音",
            ["ja"] = "消音中",
            ["ko"] = "음소거",
            ["hi"] = "म्यूट",
            ["ar"] = "مكتوم",
        },
        ["preset_console_pc"] = new Dictionary<string, string>
        {
            ["es"] = "Consola o PC",
            ["en"] = "Console or PC",
            ["de"] = "Konsole oder PC",
            ["pt"] = "Console ou PC",
            ["fr"] = "Console ou PC",
            ["zh"] = "主机或电脑",
            ["ja"] = "ゲーム機 / PC",
            ["ko"] = "콘솔 또는 PC",
            ["hi"] = "कंसोल या पीसी",
            ["ar"] = "منصة ألعاب أو كمبيوتر",
        },
        ["preset_game_console"] = new Dictionary<string, string>
        {
            ["es"] = "Consola de videojuegos",
            ["en"] = "Game console",
            ["de"] = "Spielkonsole",
            ["pt"] = "Console de videogame",
            ["fr"] = "Console de jeux",
            ["zh"] = "游戏主机",
            ["ja"] = "ゲーム機",
            ["ko"] = "게임 콘솔",
            ["hi"] = "गेम कंसोल",
            ["ar"] = "منصة ألعاب",
        },
        ["preset_streaming"] = new Dictionary<string, string>
        {
            ["es"] = "Dispositivo de streaming",
            ["en"] = "Streaming device",
            ["de"] = "Streaming-Gerät",
            ["pt"] = "Dispositivo de streaming",
            ["fr"] = "Appareil de streaming",
            ["zh"] = "流媒体设备",
            ["ja"] = "ストリーミング機器",
            ["ko"] = "스트리밍 기기",
            ["hi"] = "स्ट्रीमिंग डिवाइस",
            ["ar"] = "جهاز بث",
        },
        ["preset_pc"] = new Dictionary<string, string>
        {
            ["es"] = "Ordenador / Steam",
            ["en"] = "Computer / Steam",
            ["de"] = "Computer / Steam",
            ["pt"] = "Computador / Steam",
            ["fr"] = "Ordinateur / Steam",
            ["zh"] = "电脑 / Steam",
            ["ja"] = "PC / Steam",
            ["ko"] = "컴퓨터 / Steam",
            ["hi"] = "कंप्यूटर / स्टीम",
            ["ar"] = "كمبيوتر / ستيم",
        },
        ["preset_player"] = new Dictionary<string, string>
        {
            ["es"] = "Reproductor multimedia",
            ["en"] = "Media player",
            ["de"] = "Medienplayer",
            ["pt"] = "Reprodutor de mídia",
            ["fr"] = "Lecteur multimédia",
            ["zh"] = "多媒体播放器",
            ["ja"] = "メディアプレーヤー",
            ["ko"] = "미디어 플레이어",
            ["hi"] = "मीडिया प्लेयर",
            ["ar"] = "مشغل وسائط",
        },
        ["preset_tv_box"] = new Dictionary<string, string>
        {
            ["es"] = "Decodificador de TV",
            ["en"] = "Set-top box",
            ["de"] = "TV-Receiver",
            ["pt"] = "Conversor / TV Box",
            ["fr"] = "Décodeur TV",
            ["zh"] = "机顶盒",
            ["ja"] = "セットトップボックス",
            ["ko"] = "셋톱박스",
            ["hi"] = "सेट-टॉप बॉक्स",
            ["ar"] = "جهاز استقبال",
        },
        ["preset_device"] = new Dictionary<string, string>
        {
            ["es"] = "Dispositivo externo",
            ["en"] = "External device",
            ["de"] = "Externes Gerät",
            ["pt"] = "Dispositivo externo",
            ["fr"] = "Périphérique externe",
            ["zh"] = "外部设备",
            ["ja"] = "外部機器",
            ["ko"] = "외부 기기",
            ["hi"] = "बाहरी डिवाइस",
            ["ar"] = "جهاز خارجي",
        },
    };

    public static string T(string key, string lang = "es")
    {
        if (string.IsNullOrEmpty(key)) return "";
        if (Strings.TryGetValue(key, out var dict))
        {
            if (!string.IsNullOrEmpty(lang) && dict.TryGetValue(lang, out var val))
                return val;
            if (dict.TryGetValue("en", out var enVal))
                return enVal;
            if (dict.TryGetValue("es", out var esVal))
                return esVal;
        }
        return key;
    }

    public static CultureInfo GetCulture(string code)
    {
        try {
            switch(code) {
                case "es": return new CultureInfo("es-ES");
                case "en": return new CultureInfo("en-US");
                case "de": return new CultureInfo("de-DE");
                case "pt": return new CultureInfo("pt-PT");
                case "fr": return new CultureInfo("fr-FR");
                case "zh": return new CultureInfo("zh-CN");
                case "ja": return new CultureInfo("ja-JP");
                case "ko": return new CultureInfo("ko-KR");
                case "hi": return new CultureInfo("hi-IN");
                case "ar": return new CultureInfo("ar-SA");
                default: return CultureInfo.CurrentCulture;
            }
        } catch {
            return CultureInfo.CurrentCulture;
        }
    }

    public static string FormatDate(DateTime dt, string code)
    {
        var ci = GetCulture(code);
        return dt.ToString("ddd, MMM d · h:mm tt", ci);
    }
}