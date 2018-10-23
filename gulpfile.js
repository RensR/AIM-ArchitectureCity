/// <binding BeforeBuild='copy-assets' AfterBuild='min' Clean='clean' />
"use strict";

var _      = require("lodash"),
    gulp   = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    pump   = require("pump");

var webroot = "wwwroot/",
    modules = "node_modules/";

var paths = {
    js: webroot + "js/**/*.js",
    css: webroot + "css/**/*.css",
    concatJsDest: webroot + "js/bundle.min.js",
    concatCssDest: webroot + "css/bundle.min.css"
};

var excludedPaths = {
    minCss: webroot + "css/**/*.min.css",
    minJs: webroot + "js/**/*.min.js",
    pageJs: webroot + "js/pages/**",
    pageCss: webroot + "css/pages/**"
};

gulp.task("clean:js", function (cb) {
    rimraf(paths.concatJsDest, cb);
});

gulp.task("clean:css", function (cb) {
    rimraf(paths.concatCssDest, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("min:js", function () {
    return pump([
        gulp.src([paths.js, `!${excludedPaths.minJs}`, `!${excludedPaths.pageJs}`], { base: "." }),
        concat(paths.concatJsDest),
        uglify(),
        gulp.dest(".")
    ]);
});

gulp.task("min:css", function () {
    return pump([
        gulp.src([paths.css, `!${excludedPaths.minCss}`, `!${excludedPaths.pageCss}`]),
        concat(paths.concatCssDest),
        cssmin(),
        gulp.dest(".")
    ]);
});

gulp.task("min", ["min:js", "min:css"]);

gulp.task("copy-assets", function () {
    const assets = {
        js: [
            modules + "semantic-ui/dist/semantic.js",
            modules + "jquery/dist/jquery.js"
        ],
        css: [
            modules + "semantic-ui/dist/semantic.css"
        ]
    };
    _(assets).forEach(function (assets, type) {
        gulp.src(assets).pipe(gulp.dest(webroot + type + "/lib"));
    });
});