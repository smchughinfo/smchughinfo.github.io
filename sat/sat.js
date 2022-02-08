function Tag2(props) {
  console.log(props)
  return (
    <li className="nav-item">
      <a className={"nav-link" + (props.tag.id == 1 ? " active" : "")} href="#">{props.tag.title}</a>
    </li>
  );
}

function ActivityTabs(props) {
  let tags = [
    { id: 1, title: "XXX"},
    { id: 2, title: "YYY"},
    { id: 3, title: "ZZZ"}
  ];
    
  return ( 
    <div className="card" id="activityTabs">
      <ul className="nav nav-tabs">
        { tags.map((tag) =>  <Tag2 key={tag.id} tag={tag} /> ) }     
      </ul>
      <h1>x</h1>
      <h1>x</h1>
      <h1>x</h1>
    </div>
    
  );
}

ReactDOM.render(<ActivityTabs />, document.getElementById('root'));
