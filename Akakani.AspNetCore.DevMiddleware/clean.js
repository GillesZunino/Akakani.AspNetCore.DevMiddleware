// -----------------------------------------------------------------------------------
// Copyright 2021, AKAKANI.COM
// -----------------------------------------------------------------------------------


"use strict";

const path = require("path");
const del = require("del");


const webpackProjectConstants = require("./webpack.constants.js");

var globsToDelete = webpackProjectConstants.outputPaths.map(element => {
    // del() uses globby which requires posix style paths
    return path.normalize(element).replace("\\", "/");
});

var deletedItems = del.sync(globsToDelete);
if (deletedItems.length > 0) {
    deletedItems.forEach(value => {
        console.log(`Deleted ${value}` );
    });
}
