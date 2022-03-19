class Test extends React.Component {
    constructor(props) {
        super(props);

        this.populateData = this.populateData.bind(this);
        this.renderTest = this.renderTest.bind(this);

        this.state = {
            isLoading: true,
            passingInfo: {
                isStarted: false,
                timeStarted: null
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
        await fetch(`/tests/pass-test-info${testId}`).then(async response => {

            if (response.status === 200) {

                const result = await response.json();
                const test = result.test;
                this.setState({ test: test });

            } else window.location.replace('/home/index');

        });
    }

    renderTest() {
        const test = this.state.test, currentQuestion = this.state.currentQuestion;
        const question = test.questions[currentQuestion];
        return (<div>
            <h2 class="text-center display-2">Тест {test.testName}</h2>
            ?
            <Question number={question.number} value={question.value} />
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