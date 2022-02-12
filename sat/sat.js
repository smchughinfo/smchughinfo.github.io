function Tab(props) {
  function showTab(e) {
    $(e.target).tab("show");
  }

  var href = "#" + encodeURIComponent(props.tagName); // this is tightly coupled with ActivityList
  return (
    <li className="nav-item">
      <a onClick={showTab} className={"nav-link" + (props.activeActivity == props.tagName ? " active" : "")} href={href}>{props.tagName}</a>
    </li>
  );
}

function ActivityInfoDialog(props) {
  function setMinus60() { $("#tMinusHours").val("1"); $("#tMinusMinutes").val("0"); }
  function setMinus45() { $("#tMinusHours").val("0"); $("#tMinusMinutes").val("45"); }
  function setMinus30() { $("#tMinusHours").val("0"); $("#tMinusMinutes").val("30"); }
  function setMinus15() { $("#tMinusHours").val("0"); $("#tMinusMinutes").val("15"); }

  function trackActivity() {
    var hours = $("#tMinusHours").val();
    var minutes = $("#tMinusMinutes").val();
    
    hours = hours.replaceAll(" ", "") == "" ? "0" : hours;
    minutes = minutes.replaceAll(" ", "") == "" ? "0" : minutes;

    hours = parseInt(hours);
    minutes = parseInt(minutes);

    hours *= 1000 * 60 * 60;
    minutes *= 1000 * 60;
    var milliseconds = hours + minutes;

    props.onTrackActivity(milliseconds);
  }

  return (
      <div id="trackActivityDialog" className="modal" role="dialog">
        <div className="modal-dialog" role="document">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title">Track Activity - {props.curActivity == null ? "" : props.curActivity.name}</h5>
              <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div className="modal-body">
              <div className="form-group">
                <label>T -</label>
                <input type="text" className="form-control" id="tMinusHours" placeholder="0" /> hours
                <input type="text" className="form-control" id="tMinusMinutes" placeholder="0" /> minutes
              </div>
              <div className="form-group quick-t">
                <button type="button" class="btn btn-info" onClick={setMinus60}>T-1 hour</button>
                <button type="button" class="btn btn-warning" onClick={setMinus45}>T-45 min</button>
                <button type="button" class="btn btn-danger" onClick={setMinus30}>T-30 min</button>
                <button type="button" class="btn btn-success" onClick={setMinus15}>T-15 min</button>                
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-primary" onClick={trackActivity}>Save changes</button>
              <button type="button" className="btn btn-secondary" data-dismiss="modal">Close</button>
            </div>
          </div>
        </div>
      </div>
  );
}

function Activity(props) {
  function openActivityInfoDialog() {
    props.showTrackActivityDialog(props.activity);
  }

  return (
      <button type="button" className="activity btn btn-primary" onClick={openActivityInfoDialog}>{props.activity.name}</button>
  );
}

function ActivityList(props) {
  var id = encodeURIComponent(props.groupedActivity[0].tag); // this is tightly coupled with Tab
  return (
    <div className={"tab-pane fade" + (props.activeActivity == props.groupedActivity[0].tag ? " active show" : "")} id={id} role="tabpanel">
      { props.groupedActivity.map((activity, i) => <Activity key={i} activity={activity} showTrackActivityDialog={props.showTrackActivityDialog} />) }
    </div>
  );
}

function AddActivityDialog(props) {
  function addActivity() {
    var newActivityName = $("#newActivityName").val();
    var newActivityTag = $("#newActivityTag").val();
    props.onAddActivity({
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
        { groupedActivities.map((groupedActivity, i) => <ActivityList key={i} activeActivity={activeActivity} groupedActivity={groupedActivity} showTrackActivityDialog={props.showTrackActivityDialog} />) }    
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
  const [activities, setActivities] = React.useState(activityStorageManager.getActivities());
  const [curActivity, setCurActivity] = React.useState(null);

  function showAddActivityDialog() {
    $('#addActivityDialog').modal('show');
  }

  function addActivity(result) {
    $('#addActivityDialog').modal('hide');
    var isValid = result.name.replaceAll(" ", "") !== "" && result.tag.replaceAll(" ", "") !== "";
    if(isValid) {
      activityStorageManager.addActivity(result);
      setActivities(activityStorageManager.getActivities());
    }
  }

  function showTrackActivityDialog(activity) {
    setCurActivity(activity);
    $("#tMinusHours").val("");
    $("#tMinusMinutes").val("");
    $("#trackActivityDialog").modal('show');
  }

  function trackActivity(tMinusMilliseconds) {
    console.log(curActivity, tMinusMilliseconds);
  }

  return (
    <div>
      <ActivityTabs activities={activities} showAddActivityDialog={showAddActivityDialog} showTrackActivityDialog={showTrackActivityDialog} />
      <AddActivityDialog onAddActivity={addActivity} />
      <ActivityInfoDialog curActivity={curActivity} onTrackActivity={trackActivity} />
    </div>
  );
}

ReactDOM.render(<SAT />, document.getElementById('root'));
