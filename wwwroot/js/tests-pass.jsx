class Test extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            isLoading: true,
            passingInfo: {
                isStarted: false,
                timeStarted: null,
                timeEnd: null,
                timeLeft: null,
                isTimeOut: false,
                interval: null,
                isEnded: false,
                isCheckingAnswers: false
            },
            test: null,
            answers: [],
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

    populateData = async () => {
        const testId = this.props.testId;
        await fetch(`/api/pass/info?id=${testId}`).then(async response => {

            if (response.status === 200) {
                const object = await response.json();
                console.log(`result`);
                console.log(object);
                const test = object.result;
                const answers = [];
                for (const question of test.questions) {
                    const answer = new UserAnswer(question.number, null);
                    answers.push(answer);
                }
                this.setState({ isLoading: false, test: test, answers: answers });

            } else alert(`status: ${response.status}`);//window.location.replace('/home/index');
        });
    }

    renderTest = () => {
        const { _, passingInfo, test } = this.state;
        const timeInfo = test.timeInfo;

        const timeLimitString = getCompleteTimeString(timeInfo.hours * 3600 + timeInfo.minutes * 60 + timeInfo.seconds);
        return (<div>
            <h3 className="text-center display-3">Тест {test.testName}</h3>

            {passingInfo.isStarted
                ? passingInfo.isTimeOut
                    ? <h1 className="text-center">Время вышло!</h1>
                    : passingInfo.isEnded
                        ? null
                        : this.renderActiveTest()
                : <div className="text-center">
                    {timeInfo.isTimeLimited
                        ? <h5>Ограничение по времени: {timeLimitString}</h5>
                        : <h5>Ограничения по времени нет</h5>
                    }
                    </div>
            }
            {passingInfo.isEnded
                ? this.renderAfterTestInfo()
                : null
            }
            <div className="text-center">
            {passingInfo.isStarted
                ? passingInfo.isEnded
                    ? <button className="btn btn-outline-success" onClick={this.handleSubmit}>Отправить решение</button>
                    : null
                : <button className="btn btn-outline-primary" onClick={this.handleStart}>Начать</button>
                }
            </div>

        </div>);
    }

    renderActiveTest = () => {
        const { _, __, test, currentQuestion, answers } = this.state;
        const question = test.questions[currentQuestion];
        let answer;
        for (const a of answers) {
            if (a.questionNumber === question.number) {
                answer = a;
                break;
            }
        }
        return <div>
            {test.timeInfo.isTimeLimited
                ? <Timer onTimeOut={this.handleTimeout} timeInfo={test.timeInfo} />
                : null} 
            <Question info={question} onAnswerChanged={this.handleAnswerChange} userAnswer={answer.value} isBrowsing={false} />
            {this.renderQuestionsNavigation()}
            <button className="btn btn-outline-success"
                onClick={this.handleSave}>Закончить прохождение теста</button>
        </div>
    }
    renderQuestionsNavigation = () => {
        const test = this.state.test,
            currentQuestion = this.state.currentQuestion;

        return <div className="btn-toolbar">
            <div className="btn-group">
                <button className="btn btn-outline-primary" onClick={this.handlePrevQuestion}
                    disabled={currentQuestion === 0}>Предыдущий вопрос</button>

                <button className="btn btn-outline-primary"
                    onClick={this.handleNextQuestion}
                    disabled={currentQuestion >= test.questions.length - 1}>Следующий вопрос</button>
            </div>
        </div>
    }
    renderAfterTestInfo = () => {
        const { _, passingInfo, test, answers, currentQuestion } = this.state;
        const question = test.questions[currentQuestion];
        let answer;
        for (const a of answers) {
            if (a.questionNumber === question.number) {
                answer = a;
                break;
            }
        }
        console.log(`answer`); console.log(answer);
        const isCheckingAnswers = passingInfo.isCheckingAnswers;
        return <div>
            {isCheckingAnswers
                ? <div>
                    <Question info={question} isBrowsing={true} userAnswer={answer.value} />
                    {this.renderQuestionsNavigation()}
                    </div>
                : null
            }
            <button className="btn btn-outline-success"
                onClick={e=>this.setState({ passingInfo: { ...passingInfo, isCheckingAnswers: !isCheckingAnswers } })}>
                {isCheckingAnswers
                    ? "Закончить просматривать ответы"
                    : "Просмотреть свои ответы"
                    }
            </button>
        </div>
    }

    handleAnswerChange = (questionNumber, value) => {
        const answers = this.state.answers;
        for (const answer of answers) {
            if (answer.questionNumber === questionNumber) {
                console.log('found');
                answer.value = value;
                this.setState({ answers: answers });
                break;
            }
        }
    }

    handleNextQuestion = event => {
        event.preventDefault();

        let { _, __, test, currentQuestion } = this.state;
        console.log(test.questions.length);
        console.log(currentQuestion);
        if (test.questions.length - 1 !== currentQuestion) {
            currentQuestion++;
            this.setState({ currentQuestion: currentQuestion });
        }
    }

    handlePrevQuestion = event => { 
        event.preventDefault();

        let currentQuestion  = this.state.currentQuestion;
        if (currentQuestion > 0) {
            currentQuestion--;
            this.setState({ currentQuestion: currentQuestion });
        }
    }

    handleStart = async event => {
        event.preventDefault();

        const passingInfo = this.state.passingInfo;
        this.setState({ passingInfo: { ...passingInfo, isStarted: true } });
    }

    handleSave = event => {
        event.preventDefault();
        if (confirm("Вы уверены, что хотите закончить прохождение теста?"))
            this.setState({ passingInfo: { ...this.state.passingInfo, isEnded: true }, currentQuestion: 0 });
    }

    handleSubmit = async event => {
        //todo
    }

    handleTimeout = () => {
        const passingInfo = this.state.passingInfo;
        this.setState({ passingInfo: { ...passingInfo, isTimeOut: true, isEnded: true } });
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
        let { interval, timeLeft } = this.state;
        const { onTimeOut, timeInfo } = this.props;
        const now = new Date().getTime();
        const end = now + timeInfo.hours * 3600 * 1000 + timeInfo.minutes * 60 * 1000 + timeInfo.seconds * 1000;
        timeLeft = end - now;

        interval = setInterval(() => {

            if (timeLeft <= 0) {
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
        console.log(allSeconds);
        const hours = Math.floor(allSeconds / 3600);
        const hh = hours === 0 ? "00" : Math.floor(hours / 10) === 0 ? `0${hours}` : hours;

        const minutes = Math.floor((allSeconds - hours * 3600) / 60);
        const mm = minutes === 0 ? "00" : Math.floor(minutes / 10) === 0 ? `0${minutes}` : minutes;

        const seconds = allSeconds - hours * 3600 - minutes * 60;
        const ss = seconds === 0 ? "00" : Math.floor(seconds / 10) === 0 ? `0${seconds}` : seconds;

        const timeLeftString = `${hh}:${mm}:${ss}`;
        return (<div>
            {allSeconds === 0
                ? null
                : allSeconds < 60
                    ? <div className="text-danger">
                        <b>Осталось меньше минуты!</b>
                        <p>{timeLeftString}</p>
                    </div>
                    : <p>Осталось: {timeLeftString}</p>
            }
        </div>);
    }
}

class Question extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        const info = this.props.info, userAnswer = this.props.userAnswer, isBrowsing = this.props.isBrowsing;
        console.log(isBrowsing);
        return (<div>
            <h3>Вопрос {info.number}</h3>
            <h4 className="display-3">{info.value}</h4>
            {info.answerType === 1
                ? <div className="form-group">
                    <input type="text" className="form-control" placeholder="Ваш ответ"
                        onChange={this.onAnswerChanged} defaultValue={userAnswer} readOnly={isBrowsing}/>
                </div>
                : <div>
                    {info.answers.map(answer =>
                        <div key={answer.number} className="form-check">
                            {isBrowsing
                                ? userAnswer === answer.number
                                    ? <label className="form-check-label">Ваш ответ:</label>
                                    : null
                                : null
                            }
                            <input className="form-check-input" type="radio" name={`radio-answer`}
                                defaultValue={`${answer.number}`}
                                defaultChecked={userAnswer === answer.number}
                                onClick={isBrowsing ? e => e.preventDefault() : this.onAnswerChanged}
                                disabled={isBrowsing}
                            />
                            <label className="form-check-label">{answer.value}</label>
                        </div>
                    )
                    }
                </div>
            }
        </div>);
    }
    
    onAnswerChanged = event => {
        const questionNumber = this.props.info.number;

        let newValue = event.target.value;
        if (!isNaN(parseInt(newValue))) newValue = parseInt(newValue);

        console.log('qnumber:' + questionNumber + ', newV:' + newValue);
        this.props.onAnswerChanged(questionNumber, newValue);
    }
}

class UserAnswer {
    constructor(questionNumber, value) {
        this.questionNumber = questionNumber;
        this.value = value;
    }
}

function getCompleteTimeString(allSeconds = 0) {
    return `${getTimeString(allSeconds, "час")} ${getTimeString(allSeconds, "минут")} ${getTimeString(allSeconds, "секунд")}`;
}

function getTimeString (allSeconds = 0, starting = "") {
    starting = starting.toLowerCase();
    const hours = Math.floor(allSeconds / 3600);
    const minutes = Math.floor((allSeconds - hours * 3600) / 60);
    const seconds = allSeconds - hours * 3600 - minutes * 60;
    if (starting === 'час') {

        if (hours == 0) return "";
        if (Math.floor(hours / 10) == 1) return `${hours} часов`;
        if (hours % 10 == 1) return `${hours} час`
        if (hours % 10 > 1 && hours % 10 < 5) return `${hours} часа`;
        return `${hours} часов`;

    } else if (starting === 'минут') {

        if (minutes == 0) return "";
        if (Math.floor(minutes / 10) == 1) return `${minutes} минут`;
        if (minutes % 10 == 1) return `${minutes} минута`
        if (minutes % 10 > 1 && minutes % 10 < 5) return `${minutes} минуты`;
        return `${minutes} минут`;

    } else if (starting === 'секунд') {

        if (seconds == 0) return "";
        if (Math.floor(seconds / 10) == 1) return `${seconds} секунд`;
        if (seconds % 10 == 1) return `${seconds} секунда`
        if (seconds % 10 > 1 && seconds % 10 < 5) return `${seconds} секунды`;
        return `${seconds} секунд`;

    } else throw new Error("Некорректные значения строки. Разрешены только 'час', 'минут', 'секунд'");
};
