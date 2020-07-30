# UnityFunction

## CameraMoveLimiter

> 카메라를 기준으로 설정한 영역에 따라 오브젝트의 Position을 제한

- 주로 어떤 오브젝트의 움직임을 카메라 영역 안에서 제한하고자 할떄 사용
- 모서리 영역에 오브젝트가 존재하면 단면을 따라 오브젝트의 위치를 보정
- 지원가능영역
  - Square Pyramid (절두체)

## Firebase Addressables Remote

> Addressables 의 Remote System이 https 로 시작되는 public url만 지원하는 반면, gs를 지원하는 firebase storage 에서도 Addressables를 통한 bundle관리를 가능하게 하기 위해 Provider 를 이용해 Addressables 커스터마이징

- 참고 : <https://gitlab.com/robinbird-studios/libraries/unity-plugins/firebase-tools>
