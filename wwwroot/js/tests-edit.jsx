class EditableQuestion extends React.Component {
    constructor(props) {
        super(props);

        this.handeDelete = this.handeDelete.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);

        this.state = { deleted: false, changed: false, success: false };
    }

    render() {
        return this.state.deleted
            ? <div></div>
            : <div>
                {this.state.changed
                    ? (this.state.success
                        ? <div className="text-success">Изменения успешно сохранены</div>
                        : <div className="text-danger">Ошибка. Попробуйте ещё раз</div>)
                    : <div></div>}
                <form name={`edit-question${this.props.id}`}>
                    <h2>Вопрос {this.props.number}</h2>
                    <input type="text" className="form-control" value={this.props.value} name="value" />
                    <input type="text" className="form-control" value={this.props.answer} name="answer" />
                    <button className="btn btn-outline-success" onClick={e=> this.handleSubmit(e)}>Сохранить изменения</button>
                    <button className="btn btn-outline-danger" onClick={e => this.handeDelete(e)}>Удалить вопрос</button>
                </form>
            </div>
    }

    async handleSubmit(e) {
        e.preventDefault();
        const id = this.props.id;
        const form = document.getElementById(`edit-question${id}`);
        if (form.elements["value"].value === "" || form.elements["answer"] === "") {
            alert('Вы не можете оставить поля формы пустыми');
            return;
        }
        const formData = new FormData(form);
        formData.append('id', id);
        await fetch('/tests/update-question', {
            method: 'PUT',
            body: formData
        }).then(response => {
            this.setState({ changed: true, success: response.status === 200 ? true : false });
        })
    }

    async handleDelete(e) {
        e.preventDefault();
        const id = this.props.id;
        await fetch(`/tests/delete-question${id}`, {
            method:'DELETE'
        }).then(response => {
            if (response.status === 200) {
                this.setState({ deleted: true })
            } else {
                window.location.replace('/home/index');
            }
        });
    }
}

class EditableTest extends React.Component {
    constructor(props) {
        super(props);

        this.populateData = this.populateData.bind(this);
        this.renderTest = this.renderTest.bind(this);
        this.state = {isLoading: true, test: {}, isChanged: false, success: false };
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        const content = this.state.isLoading
            ? <h1 className="display-6">Загрузка...</h1>
            : this.renderTest()
        return (<div>
            {this.state.isChanged
                ? this.state.success
                    ? <div className="bg-success"><h1 className="display-6">Изменения сохранены</h1></div>
                    : <div className="bg-danger"><h1 className="display-6">Произошла ошибка. Попробуйте снова</h1></div>
                : <div></div>
            }
            {content}
            </div>);
    }

    async populateData() {
        const id = this.props.id;
        await fetch(`/tests/get-test${id}`).then(async response => {
            if (response.status === 200) {
                const result = await response.json();
                console.log(result);
                this.setState({
                    isLoading: false,
                    test: result
                });
            } else {
                window.location.replace('/home/index');
            }
        });
    }

    renderTest() {
        const name = this.state.test.testName,
            description = this.state.test.description,
            questions = this.state.test.questions;

        return (<div>
            <form name="edit-test" className="form-horizontal">
                <div className="form-group">
                <label className="display-6">Название теста</label>
                    <input type="text" className="form-control" name="testName" defaultValue={name} />
                </div>
                <div className="form-group">
                <label className="display-6">Описание теста</label>
                    <input type="text" className="form-control" name="description" defaultValue={description} />
                </div>
                <div className="form-group">
                    <input className="form-check-input" type="checkbox" />
                    <label className="form-check-label">Доступ только по ссылке</label>
                </div>
                <button className="btn btn-outline-success" onClick={e=>this.handleSubmit(e)}>Сохранить изменения</button>
            </form>
                <h3 className="display-6">Вопросы в тесте</h3>
                {questions.length > 0
                    ? questions.map(question => {
                    <EditableQuestion id={question.id} number={question.number} value={questions.value} answer={question.answer} />
                    })
                    : <div><p>В тесте отсутствуют вопросы.</p></div>}
        </div>);
    }

    async handleSubmit(e) {
        e.preventDefault();
        const id = this.props.id;
        const form = document.forms["edit-test"];
        if (form.elements["testName"].value === "" || form.elements["description"].value === "") {
            alert("Вы не можете оставить поля формы пустыми");
            return;
        }
        const formData = new FormData(form);
        formData.append('id', id);
        await fetch('/tests/update-test', {
            method: 'PUT',
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                const result = await response.json();
                this.setState({ test: result, isChanged: true, success: true });
            } else this.setState({ isChanged: true, success: false });
        });
    }
}