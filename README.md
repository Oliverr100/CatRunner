<p align="center">
  <a href="https://oliverrr100.itch.io/cat-runner"><img src="https://github.com/Oliverr100/Oliverr100/raw/main/assets/card-cat-runner.webp" alt="Cat Runner gameplay: a pixel-art cat in a hat runs and jumps across platforms above the sea." width="100%"></a>
</p>

# Cat Runner

An addictive endless runner starring an adorable, hat-sporting cat. Keep running, stay on the platforms and chase the highest score, but the further you go, the faster the world moves. Made in Unity with C#.

**[Play it in your browser on itch.io](https://oliverrr100.itch.io/cat-runner)**

## How to play

- **Run:** A and D, or the left and right arrows
- **Jump:** Space
- Grab coins for points. Touching an enemy or an obstacle, running into a wall or falling off the platforms ends the run. Your high score is saved.

## How it works

- `LevelGenerator.cs` builds the level from chunks 20 units wide. It spawns new chunks ahead of the cat and removes the ones far behind, so the run never ends.
- `Movement.cs` handles running, grounded jumps, wall checks and the four ways to lose: enemy, obstacle, wall and fall.
- `EnemyMovement.cs` patrols platforms and turns around at edges, and `FlyingEnemyMovement.cs` drives the flying enemies.
- `GameManager.cs` keeps the score and the saved high score, and makes the enemies faster every 3 coins.
- The `Polish` folder adds game feel: camera shake through Cinemachine impulses, sound effects synthesised in code, score pops, floating text, scene fades and a slow-motion death.

## Built with

Unity 2022.3 LTS (2022.3.62f3), C#, Cinemachine, TextMesh Pro and the Nova UI framework.

## Open the project

Clone the repo, add the folder in Unity Hub and open it with Unity 2022.3.62f3. A WebGL build is in the `WebGL_File` folder.

---

More of my games are on my [GitHub profile](https://github.com/Oliverr100) and on [itch.io](https://oliverrr100.itch.io/).
