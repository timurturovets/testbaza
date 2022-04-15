class ManageAccountForm extends React.Component {
    constructor(props) {
        super(props);
        
        this.state = {
            isLoading: true,
            changeInfo: {
                isSaved: true,
                isChanged: false,
                isSuccess: false,
                noErrors: true,
                errors: [],
            },
            user: null
        };
    }
    componentDidMount() {
        this.populateData();
    }

    render() {
        const isLoading = this.state.isLoading;
        return (
        <div>
            { isLoading
                ? <h3>Информация об аккаунте загружается...</h3>
                : this.renderForm()
            }
        </div>
        );
    }

    renderForm = () => {
        const { isChanged, isSaved, isSuccess, noErrors, errors} = this.state.changeInfo;
        return (<div>
            <h2 className="text-center">Информация об аккаунте</h2>
            <a className="btn btn-outline-success text-center" href="/profile/user-tests">Пройденные вами тесты</a>

            {isChanged
                ? isSuccess
                    ? <h3 className="text-success">Изменения успешно сохранены</h3>
                    : <h3 className="text-danger">Изменения не сохранены</h3>
                : null
            }
            <div>{errors.map(error => 
                <h3 key={error} className="text-danger">{error}</h3>
                )}
            </div>
            <form>
                {this.renderInputs()}
            </form>
            <button className="btn btn-outline-primary" onClick={e => this.handleSubmit(e)}
                disabled={isSaved || !noErrors}>Сохранить изменения</button>
        </div>);
    }
    
    renderInputs = () => {
        const user = this.state.user;
        return (<div>
            <UserNameInput defaultUsername={user.userName} handleChange={this.onNameChanged} />
            <EmailInput defaultEmail={user.email} handleChange={this.onEmailChanged} />
            <PasswordInput handleChange={this.onPasswordChanged} />
        </div>);
    }

    populateData = async () => {
        await fetch('/api/profile/user-info').then(async response => {
            if (response.status === 200) {
                const object = await response.json();
                const result = object.result;

                this.setState({ isLoading: false, user: result });
            } else {
                window.location.replace('/');
            }
        });
    }

    handleSubmit = async event => {
        event.preventDefault();
        const user = this.state.user, changeInfo = this.state.changeInfo;

        if (user.password === null || user.password === undefined || user.password === "") {
            alert('Вы не ввели пароль');
            return;
        }

        const formData = new FormData();
        formData.append('username', user.userName);
        formData.append('email', user.email);
        formData.append('password', user.password);

        await fetch('/api/profile/update-user', {
            method: 'POST',
            body: formData
        }).then(async response => {
            if (response.status === 200) {

                changeInfo.isSaved = true;
                changeInfo.isChanged = true;
                changeInfo.isSuccess = true;
                changeInfo.noErrors = true;
                changeInfo.errors = [];
                user.password = null;

                this.setState({ user: user, changeInfo: changeInfo });

            } else if (response.status === 400) {

                const object = await response.json();
                const errors = object.result;

                changeInfo.isSaved = false;
                changeInfo.isChanged = true;
                changeInfo.isSuccess = false;
                changeInfo.errors = errors;
                user.password = null;

                this.setState({ user: user, changeInfo: changeInfo });
            } else alert(`Произошла ошибка. Перезагрузите страницу (статус ${response.status})`);
        });
    }

    onNameChanged = newUserName => {
        const changeInfo = this.state.changeInfo;

        newUserName = newUserName.trim();

        if (newUserName === "" || newUserName === null) return { isSuccess: false, error: 'Вы не ввели имя' };
        if (newUserName.match(/\s/)) return { isSuccess: false, error: 'Имя не должно содержать пробела' };
        if (newUserName.length < 4) {
            changeInfo.noErrors = false;
            this.setState({ changeInfo: changeInfo });
            return { isSuccess: false, error: 'Имя должно быть длиной не менее 3 символов' };
        }
        if (newUserName.length > 20) {
            changeInfo.noErrors = false;
            this.setState({ changeInfo: changeInfo });
            return { isSuccess: false, error: 'Имя должно быть длиной не более 20 символов' };
        }

        const user = this.state.user;
        user.userName = newUserName;
        changeInfo.isSaved = false;
        changeInfo.noErrors = true;

        this.setState({ changeInfo: changeInfo, user: user });
        return { isSuccess: true, error: null };
    }

    onEmailChanged = newEmail => {
        const changeInfo = this.state.changeInfo;

        const emailRegexp = /^(?!\s)(\d|\w)+@\w+\.\w+$/ig;
        if (!emailRegexp.test(newEmail)) {
            changeInfo.noErrors = false;
            this.setState({ changeInfo: changeInfo });
            return { isSuccess: false, error: 'Вы ввели некорректный адрес эл.почты' };
        }

        const user = this.state.user;
        user.email = newEmail;

        changeInfo.isSaved = false;
        changeInfo.noErrors = true;
        this.setState({ changeInfo: changeInfo, user: user });
        return { isSuccess: true, error: null};
    }

    onPasswordChanged = newPassword => {
        const user = this.state.user;
        user.password = newPassword;
        this.setState({ isSaved: false, user: user });
        return { isSuccess: true, error: null };
    }
}

class UserNameInput extends React.Component {
    constructor(props) {
        super(props);

        this.state = { isChanged: false, isSuccess: false, error: null };
    }

    render() {
        return(
        <div className="form-group">
            {this.state.isChanged
                ? this.state.isSuccess
                    ? null
                    : <h4 className="text-danger">{this.state.error}</h4>
                : null
            }
            <label>Имя пользователя</label>
            <input type="text" className="form-control" name="username"
                defaultValue={this.props.defaultUsername} onChange={e => this.onChange(e)} />
        </div>)
    }

    onChange = event => {
        const newUsername = event.target.value;
        const { isSuccess, error } = this.props.handleChange(newUsername);
        this.setState({ isChanged: true, isSuccess: isSuccess, error: error });
    }
}

class EmailInput extends React.Component {
    constructor(props) {
        super(props);

        this.state = { isChanged: false, isSuccess: false, error: null };
    }

    render() {
        return (
            <div className="form-group">
                {this.state.isChanged
                    ? this.state.isSuccess
                        ? null
                        : <h4 className="text-danger">{this.state.error}</h4>
                    : null
                }
                <label>Эл. почта</label>
                <input type="text" className="form-control" name="email"
                    defaultValue={this.props.defaultEmail} onChange={e => this.onChange(e)} />

            </div>
        );
    }

    onChange = event => {
        const newEmail = event.target.value;
        const { isSuccess, error } = this.props.handleChange(newEmail);
        this.setState({ isChanged: true, isSuccess: isSuccess, error: error });
    }
}

class PasswordInput extends React.Component {
    constructor(props) {
        super(props);

        this.state = { isChanged: false, isSuccess: false, error: null };
    }

    render() {
        return (
            <div>
                {this.state.isChanged
                    ? this.state.isSuccess
                        ? null
                        : <h3 className="text-danger">{this.state.error}</h3>
                    : null
                }
            <div className="form-group">
                <label>Подтверждение пароля</label>
                    <input type="password" className="form-control" name="confirmpassword"
                        onChange={e=>this.onChange(e)} />
            </div>
            </div>);
    }

    onChange = event => {
        const elem = event.target;
        const value = elem.value, fieldType = elem.name;
        const { isSuccess, error } = this.props.handleChange(value, fieldType);
        this.setState({ isChanged: true, isSuccess: isSuccess, error: error })
    }
}

class PasswordChangeForm extends React.Component {
    constructor(props) {
        super(props);

        this.state = { oldPassword: null, newPassword: null, error: null };

    }

    render() {
        return (
            <form>
                <h2 className="text-center">Смена пароля</h2>
                {this.state.error === null ? null : <h3 className="text-danger">{this.state.error}</h3>}
                <div className="form-group">
                    <label>Текущий пароль</label>
                    <input type="password" className="form-control" name="oldpassword" onChange={e => this.handleChange(e, "old")} />
                </div>
                <div className="form-group">
                    <label>Новый пароль</label>
                    <input type="password" className="form-control" name="newpassword" onChange={e => this.handleChange(e, "new")} />
                </div>
                <button className="btn btn-outline-primary" onClick={e=>this.handleSubmit(e)}>Изменить пароль</button>
            </form>
            )
    }

    handleSubmit = async event => {
        event.preventDefault();
        let { oldPassword, newPassword, error } = this.state;
        if (oldPassword === null || oldPassword === undefined || oldPassword === "") {
            error = "Вы не ввели текущий пароль";
            this.setState({ error: error });
            return;
        }
        if (newPassword === null || newPassword === undefined || newPassword === "") {
            error = "Вы не ввели новый пароль";
            this.setState({ error: error });
            return;
        }
        if (oldPassword === newPassword && oldPassword !== null) {
            error = "Пароли совпадают";
            this.setState({ error: error });
            return;
        }

        const formData = new FormData();
        formData.append('oldpassword', oldPassword);
        formData.append('newpassword', newPassword);

        await fetch('/api/profile/change-password', {
            method: 'POST',
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                alert('Пароль успешно изменён');
                this.setState({oldPassword: null, newPassword: null, error: null});
            } else {
                const object = await response.json();
                const error = object.error;
                this.setState({ error: error });
            }
        });
    }

    handleChange = (event, type) => {
        const value = event.target.value;
        let error = this.state.error;
        if (type === "new") {
            const regexp = /(?!\s)(?=.*([а-яА-ЯёЁ]|[a-zA-Z]))(?=.*[0-9]).{6,30}/ig;
            if (!regexp.test(value)) {
                error = 'Пароль должен содержать от 6 до 30 символов, не иметь пробела, иметь цифры и буквы';
                this.setState({ error: error });
                return;
            } else {
                error = null;
                this.setState({ newPassword: value, error: error });
            }
        } else {
            this.setState({oldPassword: value});
        }
    }
}

class UserTests extends React.Component {
    constructor(props) {
        super(props);

        this.state = { isLoading: true, tests: [], isEmpty: false };
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        const { isLoading, tests, isEmpty } = this.state;
        console.log(tests);
        return (<div>
            <h2 className="text-center">Ваши тесты</h2>
            {isLoading
                ? <h2>Загрузка...</h2>
                : isEmpty
                    ? <div className="form-inline">
                        <label className="d-inline">У вас нет созданных тестов. </label>
                        <a className="btn btn-outline-success" href="/tests/create">Создать новый тест</a>
                    </div>
                    : <div id="tests">{tests.map(test => {
                        return (<div key={test.testId}>
                            <EditableTestSummary
                                id={test.testId}
                                name={test.testName}
                                timeCreated={test.timeCreated}
                                link={test.link}
                                isPublished={test.isPublished}
                                isBrowsable={test.isBrowsable}
                                ratesCount={test.ratesCount}
                                averageRate={test.averageRate}
                            /><hr /></div>)
                    }
                    )}
                    </div>

            }
            </div>
            );
    }

    populateData = async () => {
        let { isLoading, tests, isEmpty } = this.state;

        await fetch('/api/profile/user-tests').then(async response => {
            if (response.status === 204) {
                isEmpty = false;
                this.setState({ isLoading: false, isEmpty: true });
            } else if (response.status === 200) {
                const object = await response.json();
                const result = object.result;

                isLoading = false;
                tests = result;
                isEmpty = false;

                this.setState({ isLoading: isLoading, tests: tests, isEmpty: isEmpty })
            } else alert(`Перезагрузите страницу. (Статус ${response.status})`);
        });
    }
}

class EditableTestSummary extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        const { id,
            name,
            timeCreated,
            link,
            isPublished,
            isBrowsable,
            ratesCount,
            averageRate
        } = this.props;
        window.navigator.clipboard
        const anchor = document.createElement("a");
        anchor.setAttribute("href", `/tests/share?test=${link}`);
        const href = anchor.href;

        return (<div>
            <h3>Тест {name}</h3>
            <p>Создан {timeCreated}</p>
            {isPublished
                ? <p className="text-success d-inline">Опубликован</p>
                : <p className="text-danger d-inline">Не опубликован</p>
            }
            {isBrowsable
                ? <p className="text-success">В открытом доступе</p>
                : <p className="text-danger">Доступен только по ссылке</p>
            }
            <p>Оценили: <b>{ratesCount}</b>, средняя оценка: <b>{averageRate}</b></p>
            <p>Ссылка: <b>{href}</b></p>
            <a className="btn btn-outline-success" href={`/tests/edit${id}`}>Редактировать</a>
            </div>);
    }
}

class Forms extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        return <div>
                <ManageAccountForm />
                <hr />
                <PasswordChangeForm />
                <hr />
            <UserTests />
            </div>
    }
}

ReactDOM.render(<Forms />, document.getElementById("root"));