class Test extends React.Component {
    constructor(props) {
        super(props);

        this.populateData = this.populateData.bind(this);
        this.renderTest = this.renderTest.bind(this);
        this.handleStart = this.handleStart.bind(this);

        this.state = {
            isLoading: true,
            passingInfo: {
                isStarted: false,
                timeStarted: null,
                timeEnd: null,
                timeLeft: null,
                isTimeOut: false,
                interval: null,
                isEnded: false
            },
            test: null,
            currentQuestion: 0
        };
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        const content = this.state.isLoading
            ? (<div>
                <h2>Прохождение теста</h2>
                <h4>Загрузка...</h4>
            </div >)
            : this.renderTest();

        return content;
    }
    
    async populateData() {
        const testId = this.props.testId;
        await fetch(`/api/tests/pass-test-info${testId}`).then(async response => {

            if (response.status === 200) {
                const object = await response.json();
                console.log(`result`);
                console.log(object);
                const test = object.result
                this.setState({ isLoading: false, test: test });

            } else alert(`status: ${response.status}`)//window.location.replace('/home/index');
        });
    }

    renderTest() {
        const { _, passingInfo, test, currentQuestion } = this.state;
        const question = test.questions[currentQuestion];
        const timeInfo = test.timeInfo;

        let timeLimitString = getCompleteTimeString(timeInfo.hours*3600 + timeInfo.minutes * 60 + timeInfo.seconds);

        return (<div>
            <h3 className="display-3">Тест {test.testName}</h3>
            {passingInfo.isStarted
                ? <div>
                    <Timer onTimeOut={this.handleTimeout} timeInfo={test.timeInfo} />
                    <Question number={question.number} value={question.value} />
                </div>
                : timeInfo.isTimeLimited
                    ? <h5>Ограничение по времени: {timeLimitString}</h5>
                    :  <h5>Ограничения по времени нет</h5>
            }
            {passingInfo.isStarted
                ? null
                : <button className="btn btn-outline-primary" onClick={e=>this.handleStart(e)}>Начать</button>
            }
        </div>);
    }

    async handleStart(event) {
        event.preventDefault();

        const test = this.state.test,
            passingInfo = this.state.passingInfo;

        const now = new Date().getTime();
        passingInfo.timeStarted = now;
        console.log(test.timeInfo.seconds);
        passingInfo.timeEnd = now
            + test.timeInfo.hours * 1000 * 3600
            + test.timeInfo.minutes * 1000 * 60
            + test.timeInfo.seconds * 1000;
        passingInfo.timeLeft = passingInfo.timeEnd - passingInfo.timeStarted;

        this.setState({ passingInfo: passingInfo });
        console.log('passing info');
        console.log(passingInfo);
        const interval = setInterval(() => {
            passingInfo.timeLeft -= 1000;
            this.setState({ passingInfo: passingInfo });
            console.log(passingInfo.timeLeft);
        }, 1000);

        passingInfo.interval = interval;
        passingInfo.isStarted = true;

        this.setState({ passingInfo: passingInfo });
    }

    handleTimeout() {
        const passingInfo = this.state.passingInfo;
        passingInfo.isTimeOut = true;
        this.setState({ passingInfo: passingInfo });
    }
}

class Timer extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            interval: null,
            timeLeft: 0
        };
    }

    componentDidMount() {
        let { interval } = this.state;
        const { onTimeOut, timeInfo } = this.props;
        const now = new Date().getTime();
        const end = now + timeInfo.hours * 3600 * 1000 + timeInfo.minutes * 60 * 1000 + timeInfo.seconds * 1000;
        let timeLeft = this.state.timeLeft;
        timeLeft = end - now;

        interval = setInterval(() => {

            if (timeLeft === 0) {
                onTimeOut();
                clearInterval(interval);
                return;
            }
            timeLeft -= 1000;
            this.setState({ timeLeft: timeLeft });
        }, 1000);

        this.setState({ interval: interval });
    }

    render() {
        const timeLeft = this.state.timeLeft;
        const allSeconds = timeLeft / 1000;
        let timeLeftString = getCompleteTimeString(allSeconds);
        return (<div>
            {this.state.timeLeft * 1000 < 60
                ? <p className="text-danger">{timeLeftString}</p>
                : <p>{timeLeftString}</p>
            }
        </div>);
    }
}

class Question extends React.Component {
    constructor(props) {
        super(props);

    }

    render() {
        const { number, value } = this.props;
        return (<div>
            <h3>Вопрос {number}</h3>
            <h4>{value}</h4>
        </div>);
    }

}

function getTimeString (allSeconds = 0, starting = "") {
    starting = starting.toLowerCase();
    const hours = Math.floor(allSeconds / 3600);
    const minutes = Math.floor((allSeconds - hours * 3600) / 60);
    const seconds = allSeconds - hours * 3600 - minutes * 60;
    if (starting === 'час') {

        if (hours == 0) return "";
        if (hours / 10 == 1) return `${hours} часов`;
        if (hours % 10 == 1) return `${hours} час`
        if (hours % 10 > 1 && hours % 10 < 5) return `${hours} часа`;
        return `${hours} часов`;

    } else if (starting === 'минут') {

        if (minutes == 0) return "";
        if (minutes / 10 == 1) return `${minutes} минут`;
        if (minutes % 10 == 1) return `${minutes} минута`
        if (minutes % 10 > 1 && minutes % 10 < 5) return `${minutes} минуты`;
        return `${minutes} минут`;

    } else if (starting === 'секунд') {

        if (seconds == 0) return "";
        if (seconds / 10 == 1) return `${seconds} секунд`;
        if (seconds % 10 == 1) return `${seconds} секунда`
        if (seconds % 10 > 1 && seconds % 10 < 5) return `${seconds} секунды`;
        return `${seconds} секунд`;

    } else throw new Error("Некорректные значения строки. Разрешены только 'час', 'минут', 'секунд'");
};

function getCompleteTimeString(allSeconds) {
    return `${getTimeString(allSeconds, "час")} ${getTimeString(allSeconds, "минут")} ${getTimeString(allSeconds, "секунд")}`;
}