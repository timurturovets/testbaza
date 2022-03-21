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
                timeLeft: null,
                interval: null
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

                const result = await response.json();
                const test = result.test;
                this.setState({ test: test });

            } else window.location.replace('/home/index');

        });
    }

    renderTest() {
        const { _, passingInfo, test, currentQuestion } = this.state;
        const question = test.questions[currentQuestion];
        return (<div>
            <h2 class="text-center display-2">Тест {test.testName}</h2>
            {test.isStarted
                ? test.IsTimeLimited
                    ? <h4 className="text-success">Ограничения по времени нет</h4>
                    : (<div><h4>Ограничение по времени
                        <p className="text-danger">{new Date(test.TimeLimit * 1000).toISOString().substr(11, 8)}</p>
                    </h4>
                        <button className="btn btn-outline-sucess" onClick={e=>this.handleStart(e)}></button></div>)
                : (<div>
                    {test.IsTimeLimited
                        ? <h5>Осталось {passingInfo.timeLeft}</h5>
                        : null}
                    <Question number={question.number} value={question.value} />
                </div>)
            }

        </div>);
    }

    async handleStart(event) {
        event.preventDefault();

        const test = this.state.test,
            passingInfo = this.state.passingInfo;

        const now = new Date();
        passingInfo.timeLeft = now.setSeconds(now.getSeconds() + test.t)
        passingInfo.timeStarted = new D.getTime();

        const interval = setInterval(() => {

        }, 1000);
        passingInfo.interval = interval;
        this.setState({ passingInfo: passingInfo });
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