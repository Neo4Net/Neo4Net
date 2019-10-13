using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Helpers.progress
{
	/// <summary>
	/// A Progress object is an object through which a process can report its progress.
	/// <para>
	/// Progress objects are not thread safe, and are to be used by a single thread only. Each Progress object from a {@link
	/// ProgressMonitorFactory.MultiPartBuilder} can be used from different threads.
	/// </para>
	/// </summary>
	[Obsolete]
	public interface ProgressListener
	{
		 [Obsolete]
		 void Started( string task );

		 [Obsolete]
		 void Started();

		 [Obsolete]
		 void Set( long progress );

		 [Obsolete]
		 void Add( long progress );

		 [Obsolete]
		 void Done();

		 [Obsolete]
		 void Failed( Exception e );
	}

	public static class ProgressListener_Fields
	{
		 public static readonly ProgressListener None = new Adapter();
	}

	 public class ProgressListener_Adapter : ProgressListener
	 {
		  public override void Started()
		  {
				Started( null );
		  }

		  public override void Started( string task )
		  {
		  }

		  public override void Set( long progress )
		  {
		  }

		  public override void Add( long progress )
		  {
		  }

		  public override void Done()
		  {
		  }

		  public override void Failed( Exception e )
		  {
		  }
	 }

	 public class ProgressListener_SinglePartProgressListener : ProgressListener_Adapter
	 {
		  internal readonly Indicator Indicator;
		  internal readonly long TotalCount;
		  internal long Value;
		  internal int LastReported;
		  internal bool Stared;

		  internal ProgressListener_SinglePartProgressListener( Indicator indicator, long totalCount )
		  {
				this.Indicator = indicator;
				this.TotalCount = totalCount;
		  }

		  public override void Started( string task )
		  {
				if ( !Stared )
				{
					 Stared = true;
					 Indicator.startProcess( TotalCount );
				}
		  }

		  public override void Set( long progress )
		  {
				Update( Value = progress );
		  }

		  public override void Add( long progress )
		  {
				Update( Value += progress );
		  }

		  public override void Done()
		  {
				Set( TotalCount );
				Indicator.completeProcess();
		  }

		  public override void Failed( Exception e )
		  {
				Indicator.failure( e );
		  }

		  internal virtual void Update( long progress )
		  {
				Started();
				int current = TotalCount == 0 ? 0 : ( int )( ( progress * Indicator.reportResolution() ) / TotalCount );
				if ( current > LastReported )
				{
					 Indicator.progress( LastReported, current );
					 LastReported = current;
				}
		  }
	 }

	 public sealed class ProgressListener_MultiPartProgressListener : ProgressListener_Adapter
	 {
		  public readonly string Part;
		  public readonly long TotalCount;

		  internal readonly Aggregator Aggregator;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal bool StartedConflict;
		  internal long Value;
		  internal long LastReported;

		  internal ProgressListener_MultiPartProgressListener( Aggregator aggregator, string part, long totalCount )
		  {
				this.Aggregator = aggregator;
				this.Part = part;
				this.TotalCount = totalCount;
		  }

		  public override void Started( string task )
		  {
				if ( !StartedConflict )
				{
					 Aggregator.start( this );
					 StartedConflict = true;
				}
		  }

		  public override void Set( long progress )
		  {
				Update( Value = progress );
		  }

		  public override void Add( long progress )
		  {
				Update( Value += progress );
		  }

		  public override void Done()
		  {
				Set( TotalCount );
				Aggregator.complete( this );
		  }

		  public override void Failed( Exception e )
		  {
				Aggregator.signalFailure( e );
		  }

		  internal void Update( long progress )
		  {
				Started();
				if ( progress > LastReported )
				{
					 Aggregator.update( progress - LastReported );
					 LastReported = progress;
				}
		  }

		  internal enum State
		  {
				Init,
				Live
		  }
	 }

}