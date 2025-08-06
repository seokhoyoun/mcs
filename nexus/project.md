MCS 프로젝트 최종 설계 청사진
지금까지 논의된 MCS(Material Control System) 시스템의 핵심 내용, 기술 스택, 아키텍처 및 개발 계획을 상세하게 정리했습니다. 이 문서는 프로젝트의 명확한 방향성과 구조를 제시하는 최종 청사진 역할을 합니다.

1. 시스템 목표 및 개요
MCS 시스템은 동글마켓의 물류 창고에서 AGV(무인 운반 로봇)를 효율적으로 제어하고 관리하기 위한 핵심 소프트웨어입니다.

목표:

자재 운반 제어: 타사 시스템인 ACS(Automated Control System)와 통신하여 AGV의 움직임을 직접 제어합니다.

실시간 모니터링: UI를 통해 AGV의 실시간 상태, 운반 작업 진행 상황을 파악합니다.

영구 기록: 모든 작업 이력, 명령 상태, 이벤트 로그를 안전하게 보존합니다.

프로젝트 코드네임: 여러 요소가 연결되는 '중심점'을 의미하는 Nexus로 결정되었습니다.

2. 기술 스택
MCS의 성능, 안정성, 확장성을 고려하여 다음과 같은 기술 스택이 선정되었습니다.

언어 및 프레임워크:

C# 언어를 사용하며, ASP.NET Core의 Worker Service 템플릿을 기반으로 모든 백그라운드 서비스를 구축합니다.

통신 및 메시징:

Redis: 모든 서비스 간의 실시간 메시지 통신을 위한 중앙 허브 역할을 담당합니다. 이벤트 기반 아키텍처(Event-Driven Architecture)의 핵심입니다.

Web Socket: MCS 서비스와 ACS 간의 지속적인 양방향 통신에 사용됩니다.

데이터베이스:

PostgreSQL: 모든 운반 이력, 명령 상태, 로그 등 영구 보존이 필요한 데이터를 저장합니다.

Supabase(로컬): 개발 편의성을 위해 로컬 환경에 Docker로 구축하여 사용합니다.

UI:

Blazor: C#으로 웹 기반의 관리자 페이지를 구축합니다.

Redis와 Supabase의 데이터를 모두 구독하여 실시간 상태 및 영구 기록을 표시합니다.

3. 마이크로서비스 아키텍처
각 기능을 독립적인 프로세스(서비스)로 분리하여 안정성과 확장성을 극대화합니다.

Nexus.Integrator:

역할: ACS와 통신하여 데이터를 Redis에 발행하고, Redis에서 받은 명령을 ACS에 전달하는 역할. (타사 시스템과의 통합 담당)

Nexus.Scheduler:

역할: Redis를 구독하여 AGV 상태를 파악하고, 최적의 운반 계획 알고리즘을 실행하여 명령을 Redis에 발행하는 역할.

Nexus.DataLogger:

역할: Redis를 구독하여 중요한 명령 이력 및 이벤트 로그를 PostgreSQL에 영구적으로 저장하는 역할.

Nexus.UiHub:

역할: Redis를 구독하고 Blazor UI와 연결하여 실시간 데이터를 전달하는 역할. (로컬 Supabase의 실시간 기능으로 대체될 수 있음)

4. 공통 모델 및 비즈니스 로직 분리
공장별로 달라지는 물품과 로직을 유연하게 처리하기 위한 설계입니다.

공통 모델 프로젝트:

Nexus.Core: 모든 서비스가 공유하는 클래스 라이브러리 프로젝트.

IItem: 모든 물품(운반 단위, 내용물)의 가장 기본이 되는 인터페이스.

속성: Id, Name, Weight 등 공통 속성 포함.

ITransportUnit: 운반이 가능한 물품에만 필요한 인터페이스. IItem을 상속하고, Contents 속성으로 IItem 리스트를 포함합니다.

공장별 비즈니스 로직 분리:

전략 패턴(Strategy Pattern): Nexus.Scheduler 프로젝트에 ISchedulingStrategy와 같은 인터페이스를 정의합니다.

공장별 클래스 라이브러리: 각 공장별로 Nexus.Scheduler.FactoryA, Nexus.Scheduler.FactoryB와 같은 프로젝트를 따로 만들어, 공장별 특화된 ISchedulingStrategy 구현체를 담습니다.

동적 로딩: Nexus.Scheduler는 설정 파일을 기반으로 해당 공장 로직을 동적으로 로드하여 사용합니다.

5. 모니터링 및 개발 환경
모니터링: Prometheus와 Grafana를 Docker로 구축하여 모든 서비스의 상태를 통합적으로 모니터링합니다.

개발 환경:

Docker Compose: 모든 서비스를 한 번의 명령어로 실행하고 관리합니다.

Dockerfile: 각 C# 프로젝트의 컨테이너 이미지를 빌드합니다.

hosts 파일: 로컬 접속을 위해 mcs.dongglemarket.local과 같은 별칭을 설정합니다.