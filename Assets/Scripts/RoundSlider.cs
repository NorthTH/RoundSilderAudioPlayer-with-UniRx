using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;

[RequireComponent(typeof(Image))]
public class RoundSlider : MonoBehaviour {

	private Image round;
	private GraphicRaycaster raycaster;
	private Func<Vector3,float> returnAmount;
	
	public Subject<float> Amount = new Subject<float>();

	public bool DragMode = true;
	public bool fillEnabled = true;

	void Start () {
		round = GetComponent<Image>();
		raycaster = GetComponentInParent<GraphicRaycaster>();

		int Clockwise = 1;
		if(!round.fillClockwise)
			Clockwise = -1;

		if(round.fillMethod != Image.FillMethod.Radial360)
		{
			Debug.LogError("this script require to Use FillMethod.Radial360");
			return;
		}

		switch (round.fillOrigin)
		{
			case 0:
				returnAmount = (localPos) => {
					return (Mathf.Atan2(localPos.x * Clockwise, localPos.y)*180f/Mathf.PI+180f)/360f;
				};
				break;
			case 1:
				returnAmount = (localPos) => {
					return (Mathf.Atan2(localPos.y * Clockwise, -localPos.x)*180f/Mathf.PI+180f)/360f;
				};
				break;
			case 2:
				returnAmount = (localPos) => {
					return (Mathf.Atan2(-localPos.x * Clockwise, -localPos.y)*180f/Mathf.PI+180f)/360f;
				};
				break;
			case 3:
				returnAmount = (localPos) => {
					return (Mathf.Atan2(-localPos.y * Clockwise, localPos.x)*180f/Mathf.PI+180f)/360f;
				};
				break;
		}

		round.UpdateAsObservable()
            .SkipUntil(round.OnPointerDownAsObservable())
			.TakeUntil(round.OnPointerUpAsObservable().Do(_ => {
						// ボタンを離したときの処理
                        
					}))
			.Select(_ => 1)
			.Scan((sum, addCount) => {
				return sum + addCount;
			})
			.Repeat()
			.Subscribe( totalCount => {
				if(DragMode)
					TrackPointer();
				else
				{
					if(totalCount == 1)
						TrackPointer();
				}
		}).AddTo(this);

		Amount.Where( _ => fillEnabled)
			.Subscribe( value => {
				round.fillAmount = value;
		}).AddTo(this);
	}
	
	void TrackPointer()
	{
		Vector2 localPos; // Mouse position  
		RectTransformUtility.ScreenPointToLocalPointInRectangle( transform as RectTransform, Input.mousePosition, raycaster.eventCamera, out localPos );

		// local pos is the mouse position.
		float amount = returnAmount(localPos);

		Amount.OnNext(amount);

		//round.color = Color.Lerp(Color.green, Color.red, angle);
	}
}
