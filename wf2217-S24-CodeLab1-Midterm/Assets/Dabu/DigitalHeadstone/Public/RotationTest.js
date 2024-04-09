//@input float rotationDuration = 5.0 {"label":"Rotation Duration (Seconds)"}

var currentTime = 0;

function onUpdate(eventData) {
    
    currentTime += getDeltaTime();
    currentTime = currentTime % script.rotationDuration; 

    
    var fraction = currentTime / script.rotationDuration;
    var currentRotation = fraction * 360; 

    
    script.getSceneObject().getTransform().setLocalRotation(quat.fromEulerAngles(0, 0, currentRotation * Math.PI / 180));
}


script.createEvent("UpdateEvent").bind(onUpdate);
