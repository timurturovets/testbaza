class TestSummary extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        const { name, timeCreated, authorName, questionsCount, ratesCount, averageRate } = this.props;
        return (<div>
            <h2 className="display-2 m-0">Тест "{name}"</h2>
            <p className="m-0">Создан: <b>{timeCreated}</b></p>
            <p className="m-0">Автор: <b>{authorName}</b></p>
            <p className="m-0">Вопросов: <b>{questionsCount}</b></p>
            <p className="m-0">Оценили: <b>{ratesCount}</b>, средняя оценка: <b>{averageRate}</b></p>
            <a className="btn btn-outline-success btn-lg m-0" href={`/tests/pass?id=${this.props.id}`}>Пройти тест</a>
        </div>);
    }
}

class TestsList extends React.Component {
    constructor(props) {
        super(props);

        this.state = {isLoading: true, tests: []}
    }

    componentDidMount() {
        this.populateTests();
    }

    render() {
        const content = this.state.isLoading
            ? <h1>Доступные тесты загружаются...</h1>
            : this.renderTests();

        return content;
    }

    populateTests = async () => {
        const response = await fetch('/api/tests/all');
        if (response.status == 200) {
            await response.json().then(object => {
                console.log(object);
                this.setState({ isLoading: false, tests: object.result });
            })
        } else if (response.status === 204) this.setState({ isLoading: false, tests: [] });
        else alert(`Произошла непредвиденная ошибка. Попробуйте перезагрузить страницу. ${response.status}`);
    }

    renderTests = () => {
        console.log(this.state);
        return this.state.tests.length > 0
            ? (<div>
                {
                    this.state.tests.map(test => {
                        return(
                        <div key={test.testName}>
                            <TestSummary id={test.testId}
                                name={test.testName}
                                authorName={test.authorName}
                                questionsCount={test.questionsCount}
                                timeCreated={test.timeCreated}
                                ratesCount={test.ratesCount}
                                averageRate={test.averageRate}
                            />
                            <hr />
                        </div>)
                    })
                }
            </div>)

        : (<p>Пока-что ещё не создано ни одного теста.
                <a className="btn btn-outline-success m-1" href="/tests/create">Создайте первый! :)</a>
        </p>)
    }
}

ReactDOM.render(
    <TestsList />,
    document.getElementById('root')
)