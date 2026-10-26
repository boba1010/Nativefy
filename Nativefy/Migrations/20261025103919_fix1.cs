using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nativefy.Migrations
{
    /// <inheritdoc />
    public partial class fix1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistTracks_Tracks_TrackVideoId",
                table: "PlaylistTracks");

            migrationBuilder.DropIndex(
                name: "IX_PlaylistTracks_TrackVideoId",
                table: "PlaylistTracks");

            migrationBuilder.DropColumn(
                name: "TrackVideoId",
                table: "PlaylistTracks");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistTracks_VideoId",
                table: "PlaylistTracks",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistTracks_Tracks_VideoId",
                table: "PlaylistTracks",
                column: "VideoId",
                principalTable: "Tracks",
                principalColumn: "VideoId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistTracks_Tracks_VideoId",
                table: "PlaylistTracks");

            migrationBuilder.DropIndex(
                name: "IX_PlaylistTracks_VideoId",
                table: "PlaylistTracks");

            migrationBuilder.AddColumn<string>(
                name: "TrackVideoId",
                table: "PlaylistTracks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistTracks_TrackVideoId",
                table: "PlaylistTracks",
                column: "TrackVideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistTracks_Tracks_TrackVideoId",
                table: "PlaylistTracks",
                column: "TrackVideoId",
                principalTable: "Tracks",
                principalColumn: "VideoId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
