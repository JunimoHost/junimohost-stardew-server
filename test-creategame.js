var axios = require("axios");
var data = JSON.stringify({
  WhichFarm: 4,
});

var config = {
  method: "post",
  url: "http://localhost:8082/game",
  headers: {
    "Content-Type": "application/json",
  },
  data: data,
};

axios(config)
  .then(function (response) {
    console.log(JSON.stringify(response.data));
  })
  .catch(function (error) {
    console.log(error);
  });
