/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Neo4Net.Pushtocloud
{

	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;

	internal class ProgressTrackingOutputStream : Stream
	{
		 private readonly Stream _actual;
		 private readonly Progress _progress;

		 internal ProgressTrackingOutputStream( Stream actual, Progress progress )
		 {
			  this._actual = actual;
			  this._progress = progress;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] b, int off, int len) throws java.io.IOException
		 public override void Write( sbyte[] b, int off, int len )
		 {
			  _actual.Write( b, off, len );
			  _progress.add( len );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public override void Flush()
		 {
			  _actual.Flush();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _actual.Close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(int b) throws java.io.IOException
		 public override void Write( int b )
		 {
			  _actual.WriteByte( b );
			  _progress.add( 1 );
		 }

		 internal class Progress
		 {
			  internal readonly ProgressListener UploadProgress;
			  // Why have this as a separate field here? Because we will track local progress while streaming the file,
			  // i.e. how much we send. But if the upload gets aborted we may take a small step backwards after asking about resume position
			  // and so to play nice with out progress listener (e.g. hard to remove printed dots from the terminal)
			  // we won't report until we're caught up with it.
			  internal long HighestReportedProgress;
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal long ProgressConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool DoneConflict;

			  /// <param name="progressListener"> <seealso cref="ProgressListener"/> to report upload progress to. </param>
			  /// <param name="position"> initial position to start the upload from. This is only useful if the upload was started and made it part-way
			  /// there before the command failed and the command has to be reissued at which point it can be resumed. This position is the position
			  /// where the upload will continue from. This is separate from temporary failure where the upload will be retried after some back-off.
			  /// That logic will instead make use of <seealso cref="rewindTo(long)"/>. </param>
			  internal Progress( ProgressListener progressListener, long position )
			  {
					UploadProgress = progressListener;
					if ( position > 0 )
					{
						 UploadProgress.add( position );
					}
			  }

			  internal virtual void Add( int increment )
			  {
					ProgressConflict += increment;
					if ( ProgressConflict > HighestReportedProgress )
					{
						 UploadProgress.add( ProgressConflict - HighestReportedProgress );
						 HighestReportedProgress = ProgressConflict;
					}
			  }

			  internal virtual void RewindTo( long absoluteProgress )
			  {
					// May be lower than what we're at, but that's fine
					ProgressConflict = absoluteProgress;
					// highestReportedProgress will be kept as it is so that we know when we're caught up to it once more
			  }

			  internal virtual void Done()
			  {
					DoneConflict = true;
					UploadProgress.done();
			  }

			  internal virtual bool Done
			  {
				  get
				  {
						return DoneConflict;
				  }
			  }
		 }
	}

}