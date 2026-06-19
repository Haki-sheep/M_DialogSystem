using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class StandUIController : MonoBehaviour
{
    [BoxGroup("黑边参数"), SerializeField]
    private Image upPage;
    [BoxGroup("黑边参数"), SerializeField]
    private Image downPage;
    [BoxGroup("黑边参数")] public float UpHiddenY = 80f;
    [BoxGroup("黑边参数")] public float UpShownY = -80f;
    [BoxGroup("黑边参数")] public float DownHiddenY = -80f;
    [BoxGroup("黑边参数")] public float DownShownY = 80f;


    [BoxGroup("主画面"), SerializeField]
    private Image mainImage;
    [BoxGroup("主画面"), SerializeField, LabelText("打字机 字/秒")]
    private float typewriterSpeed = 40f;

    [BoxGroup("选项参数"), SerializeField, LabelText("选项倒计时")]
    private Image opCountDownImage;

    [BoxGroup("选项参数"), SerializeField]
    private Button[] optionButtonList;

    [SerializeField, LabelText("动画时长")]
    private float duration = 0.5f;


    // 私有字段 
    private RectTransform UpPageRectTransform => upPage.rectTransform;
    private RectTransform DownPageRectTransform => downPage.rectTransform;
    private TextMeshProUGUI[] textMeshProUGUIList;
    private TextMeshProUGUI downPageText;
    private Tweener typewriterTween;

    // 属性
    public TextMeshProUGUI[] TextMeshProUGUIList => textMeshProUGUIList;
    void Start()
    {
        HidePage();
        MainImageHide();

        if (downPageText == null && downPage != null)
            downPageText = downPage.GetComponentInChildren<TextMeshProUGUI>();

        if (optionButtonList is not null)
        {
            textMeshProUGUIList = new TextMeshProUGUI[optionButtonList.Length];
            for (int i = 0; i < optionButtonList.Length; i++)
            {
                textMeshProUGUIList[i] = optionButtonList[i].GetComponentInChildren<TextMeshProUGUI>();
            }
        }
    }

    void Update()
    {
        // 测试代码
        if (Input.GetKeyDown(KeyCode.O))
            HidePage();
        if (Input.GetKeyDown(KeyCode.P))
            ShowPage();
        if (Input.GetKeyDown(KeyCode.A))
            ShowOptions(0, 1, 2, 3);
        if (Input.GetKeyDown(KeyCode.B))
            HideOptions(0, 1, 2, 3);
        if (Input.GetKeyDown(KeyCode.C))
            SetOpCountDown(3);
        if (Input.GetKeyDown(KeyCode.D))
            SetTypewriterText("this is a test text");
    }


    void OnDestroy()
    {
        StopTypewriter();
        KillTweens();
    }

    #region 外部控制
    /// <summary>
    /// 设置选项文本
    /// </summary>
    /// <param name="index">选项索引</param>
    /// <param name="text"></param>
    public void SetOptionText(int index, string text)
    {
        if (textMeshProUGUIList is null) return;
        if (index < 0 || index >= textMeshProUGUIList.Length) return;
        textMeshProUGUIList[index].text = text;
    }

    /// <summary>
    /// 设置并立即显示整段文本
    /// </summary>
    /// <param name="text"></param>
    public void SetImmediateText(string text)
    {
        StopTypewriter();
        if (downPageText == null)
            return;
        downPageText.text = text;
    }

    /// <summary>
    /// 设置并以打字机效果显示文本
    /// </summary>
    public void SetTypewriterText(string text, Action onComplete = null)
    {
        if (downPageText == null)
            return;

        StopTypewriter();
        downPageText.text = string.Empty;

        if (string.IsNullOrEmpty(text))
        {
            onComplete?.Invoke();
            return;
        }

        // 计算打字机时长
        float typeDuration = text.Length / Mathf.Max(typewriterSpeed, 1f);
        // 设置打字机动画
        typewriterTween = downPageText
            .DOText(text, typeDuration, true)
            .SetEase(Ease.Linear)
            .OnComplete(() => onComplete?.Invoke());

    }

    /// <summary>
    /// 立刻显示打字机剩余全文
    /// </summary>
    public void CompleteTypewriter()
    {
        typewriterTween?.Complete();
    }

    void StopTypewriter()
    {
        downPageText?.DOKill();
        typewriterTween = null;
    }

    #endregion

    #region 对话选项控制

    /// <summary>
    /// 设置选项倒计时
    /// </summary>
    /// <param name="count"></param>
    public void SetOpCountDown(float countDownTotalTime)
    {
        opCountDownImage.gameObject.SetActive(true);
        opCountDownImage.fillAmount = 1;

        opCountDownImage.DOFillAmount(0, countDownTotalTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            opCountDownImage.gameObject.SetActive(false);
        });

    }
    /// <summary>
    /// 显示选项
    /// </summary>
    /// <param name="optionIndexIndexes"></param>
    public void ShowOptions(params int[] optionIndexIndexes)
    {
        foreach (var item in optionIndexIndexes)
        {
            if (item < 0 || item >= optionButtonList.Length) continue;
            optionButtonList[item].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏选项
    /// </summary>
    /// <param name="optionIndexIndexes"></param>
    public void HideOptions(params int[] optionIndexIndexes)
    {
        foreach (var item in optionIndexIndexes)
        {
            if (item < 0 || item >= optionButtonList.Length) continue;
            optionButtonList[item].gameObject.SetActive(false);
        }
    }


    #endregion

    #region 主画面是否显示

    /// <summary>
    /// 显示主画面
    /// </summary>
    public void MainImageShow()
    {
        float width = Screen.width;
        float height = Screen.height - upPage.rectTransform.rect.height - downPage.rectTransform.rect.height;
        mainImage.rectTransform.sizeDelta = new Vector2(width, height);

        mainImage.DOFade(1, duration).SetEase(Ease.OutCubic);
    }


    /// <summary>
    /// 隐藏主画面
    /// </summary>
    public void MainImageHide()
    {
        mainImage.DOFade(0, duration).SetEase(Ease.InCubic).OnComplete(() =>
        {
            mainImage.rectTransform.sizeDelta = new Vector2(0, 0);
        });

    }

    #endregion

    #region 电影黑边

    /// <summary>
    /// 滑入显示
    /// </summary>
    public void ShowPage(bool isShowMainImage = true)
    {
        if (isShowMainImage)
            MainImageShow();
        UpPageRectTransform.DOAnchorPos(new Vector2(UpPageRectTransform.anchoredPosition.x, UpShownY), duration).SetEase(Ease.OutCubic);
        DownPageRectTransform.DOAnchorPos(new Vector2(DownPageRectTransform.anchoredPosition.x, DownShownY), duration).SetEase(Ease.OutCubic);
    }

    /// <summary>
    /// 滑出隐藏
    /// </summary>
    public void HidePage(bool isHideMainImage = true)
    {
        if (isHideMainImage)
            MainImageHide();
        UpPageRectTransform.DOAnchorPos(new Vector2(UpPageRectTransform.anchoredPosition.x, UpHiddenY), duration).SetEase(Ease.InCubic);
        DownPageRectTransform.DOAnchorPos(new Vector2(DownPageRectTransform.anchoredPosition.x, DownHiddenY), duration).SetEase(Ease.InCubic);
    }

    /// <summary>
    /// 清理残留动画
    /// </summary>
    void KillTweens()
    {
        UpPageRectTransform.DOKill();
        DownPageRectTransform.DOKill();
        mainImage.DOKill();
        opCountDownImage.DOKill();
    }

    #endregion

}
