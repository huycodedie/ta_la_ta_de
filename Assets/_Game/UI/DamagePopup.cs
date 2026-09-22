using TMPro;
using UnityEngine;
using WuxiaGame.Combat;

namespace WuxiaGame.UI
{
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private float floatSpeed = 2.2f;
        [SerializeField] private float fadeDuration = 0.9f;

        private float timer = 0f;
        private Color textColor;
        private Vector3 moveVector;
        private Vector3 initialScale;

        private void Awake()
        {
            initialScale = transform.localScale;
        }

        public void Setup(DamageResult result)
        {
            if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
            if (initialScale == Vector3.zero) initialScale = transform.localScale;

            if (result.IsDodged)
            {
                textMesh.text = "<i>NÉ ĐÒN</i>";
                textColor = new Color(0.75f, 0.78f, 0.85f, 1f);
                textMesh.fontSize = 5.5f;
            }
            else if (result.IsCrit)
            {
                textMesh.text = $"<size=110%>★ BẠO KÍCH! ★</size>\n<size=130%>{Mathf.CeilToInt(result.FinalDamage)}</size>";
                textColor = new Color(1.00f, 0.84f, 0.20f, 1f); // Bright Antique Gold
                textMesh.fontSize = 6.5f;
                transform.localScale = initialScale * 1.25f;
            }
            else
            {
                if (result.DamageType == DamageType.Ultimate)
                {
                    textMesh.text = $"<size=115%>THẦN CÔNG {Mathf.CeilToInt(result.FinalDamage)}</size>";
                    textColor = new Color(1.00f, 0.55f, 0.15f, 1f);
                    textMesh.fontSize = 6f;
                }
                else if (result.DamageType == DamageType.Skill)
                {
                    textMesh.text = $"{Mathf.CeilToInt(result.FinalDamage)}";
                    textColor = new Color(0.30f, 0.85f, 1.00f, 1f);
                    textMesh.fontSize = 5.5f;
                }
                else
                {
                    textMesh.text = $"{Mathf.CeilToInt(result.FinalDamage)}";
                    textColor = Color.white;
                    textMesh.fontSize = 5.2f;
                }
            }

            textMesh.color = textColor;
            timer = 0f;
            moveVector = new Vector3(Random.Range(-0.35f, 0.35f), floatSpeed, 0);
        }

        public void SetupShieldAbsorb(float absorbAmount)
        {
            if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
            if (initialScale == Vector3.zero) initialScale = transform.localScale;

            textMesh.text = $"<size=90%>HỘ THỂ -{Mathf.CeilToInt(absorbAmount)}</size>";
            textColor = new Color(0.25f, 0.75f, 1.00f, 1f); // Qi Blue
            textMesh.fontSize = 4.8f;
            textMesh.color = textColor;
            timer = 0f;
            moveVector = new Vector3(Random.Range(-0.25f, 0.25f), floatSpeed * 0.8f, 0);
        }

        private void Update()
        {
            transform.position += moveVector * Time.deltaTime;
            moveVector -= moveVector * (1.2f * Time.deltaTime);

            timer += Time.deltaTime;
            if (timer >= fadeDuration)
            {
                float alpha = 1f - ((timer - fadeDuration) / 0.3f);
                textMesh.color = new Color(textColor.r, textColor.g, textColor.b, alpha);

                if (alpha <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
