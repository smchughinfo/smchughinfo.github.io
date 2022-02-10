function Tab(props) {
  function showTab(e) {
    $(e.target).tab("show");
  }

  var href = "#" + encodeURIComponent(props.tagName); // this is tightly coupled ActivityList
  return (
    <li className="nav-item">
      <a onClick={showTab} className={"nav-link" + (props.activeActivity == props.tagName ? " active" : "")} href={href}>{props.tagName}</a>
    </li>
  );
}

function Activity(props) {
  return (
    <button type="button" className="activity btn btn-primary">{props.name}</button>
  );
}

function ActivityList(props) {
  var id = encodeURIComponent(props.groupedActivity[0].tag); // this is tightly coupled Tab
  return (
    <div className={"tab-pane fade" + (props.activeActivity == props.groupedActivity[0].tag ? " active show" : "")} id={id} role="tabpanel">
      { props.groupedActivity.map((activity, i) => <Activity key={i} {...activity}/>) }
    </div>
  );
}

function AddActivity(props) {
  function addActivity() {
    var newActivityName = $("#newActivityName").val();
    var newActivityTag = $("#newActivityTag").val();
    props.addActivity({
      name: newActivityName,
      tag: newActivityTag
    });
  }

  return(
    <div id="addActivityDialog" className="modal" role="dialog">
      <div className="modal-dialog" role="document">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">Add Activity</h5>
            <button type="button" className="close" data-dismiss="modal" aria-label="Close">
              <span aria-hidden="true">&times;</span>
            </button>
          </div>
          <div className="modal-body">
            <div className="form-group">
              <label htmlFor="newActivityName">Activity Name</label>
              <input type="text" className="form-control" id="newActivityName" placeholder="1.5 Mile Jog, 4 Hours Programming, 1 Hour Math, etc." />
            </div>
            <div className="form-group">
              <label htmlFor="newActivityTag">Group</label>
              <input type="text" className="form-control" id="newActivityTag" placeholder="Fitness, Work, Learning, etc." />
            </div>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-primary" onClick={addActivity}>Save changes</button>
            <button type="button" className="btn btn-secondary" data-dismiss="modal">Close</button>
          </div>
        </div>
      </div>
    </div>
  );
}

function ActivityTabs(props) {
  let tags = [...new Set(props.activities.map(activity => activity.tag))]; // https://stackoverflow.com/a/35092559
  let groupedActivities = tags.map(tag => props.activities.filter(activity => activity.tag == tag));
  let activeActivity = tags[0];

  return ( 
    <div className="card" id="activityTabs">
      <ul className="nav nav-tabs">
        { tags.map((tagName, i) =>  <Tab key={i} activeActivity={activeActivity} tagName={tagName} /> ) }     
      </ul>
      <button id="buttonShowAddActivity" type="button" className="btn btn-primary" onClick={props.showAddActivityDialog}>Primary</button>
      <div className="tab-content">
        { groupedActivities.map((groupedActivity, i) => <ActivityList key={i} activeActivity={activeActivity} groupedActivity={groupedActivity}/>) }    
      </div>
    </div>
  );
}

var activityStorageManager = (function() {
  function getActivities() {
    return localStorage.activities ? JSON.parse(localStorage.activities) : [
      { name: "Play with dog", tag: "Dog Watching"},
      { name: "Stare at dog", tag: "Dog Watching"},
      { name: "Pet dog", tag: "Dog Watching"},
      { name: "Apple", tag: "Eating"},
      { name: "Frosting", tag: "Eating"},
    ];
  }
  
  function addActivity(activity) {
    let activities = getActivities();
    activities.push(activity);
    setActivities(activities);
  }
  
  function setActivities(activities) {
    localStorage.activities = JSON.stringify(activities);
  }

  return {
    getActivities: getActivities,
    addActivity: addActivity,
    setActivities: setActivities
  }
})()


function SAT(props) {
  let activities = activityStorageManager.getActivities();

  function showAddActivityDialog() {
    $('#addActivityDialog').modal('show');
  }

  function addActivity(result) {
    $('#addActivityDialog').modal('hide');
    var isValid = result.name.replaceAll(" ", "") !== "" && result.tag.replaceAll(" ", "") !== "";
    if(isValid) {
      activityStorageManager.addActivity(result);
    }
  }

  return (
    <div>
      <ActivityTabs activities={activities} showAddActivityDialog={showAddActivityDialog} />
      <AddActivity addActivity={addActivity} />
    </div>
  );
}

ReactDOM.render(<SAT />, document.getElementById('root'));
