using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

	[Obsolete]
	public abstract class ProgressMonitorFactory
	{
		 [Obsolete]
		 public static readonly ProgressMonitorFactory NONE = new ProgressMonitorFactoryAnonymousInnerClass();

		 private class ProgressMonitorFactoryAnonymousInnerClass : ProgressMonitorFactory
		 {
			 protected internal override Indicator newIndicator( string process )
			 {
				  return Indicator.NONE;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ProgressMonitorFactory textual(final java.io.OutputStream out)
		 [Obsolete]
		 public static ProgressMonitorFactory Textual( Stream @out )
		 {
			  return Textual( new StreamWriter( @out, Encoding.UTF8 ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ProgressMonitorFactory textual(final java.io.Writer out)
		 [Obsolete]
		 public static ProgressMonitorFactory Textual( Writer @out )
		 {
			  return new ProgressMonitorFactoryAnonymousInnerClass2( @out );
		 }

		 private class ProgressMonitorFactoryAnonymousInnerClass2 : ProgressMonitorFactory
		 {
			 private Writer @out;

			 public ProgressMonitorFactoryAnonymousInnerClass2( Writer @out )
			 {
				 this.@out = @out;
			 }

			 protected internal override Indicator newIndicator( string process )
			 {
				  return new Indicator.Textual( process, writer() );
			 }

			 private PrintWriter writer()
			 {
				  return @out is PrintWriter ? ( PrintWriter ) @out : new PrintWriter( @out );
			 }
		 }

		 [Obsolete]
		 public MultiPartBuilder MultipleParts( string process )
		 {
			  return new MultiPartBuilder( this, process );
		 }

		 [Obsolete]
		 public ProgressListener SinglePart( string process, long totalCount )
		 {
			  return new ProgressListener_SinglePartProgressListener( NewIndicator( process ), totalCount );
		 }

		 protected internal abstract Indicator NewIndicator( string process );

		 [Obsolete]
		 public class MultiPartBuilder
		 {
			  internal Aggregator Aggregator;
			  internal ISet<string> Parts = new HashSet<string>();

			  internal MultiPartBuilder( ProgressMonitorFactory factory, string process )
			  {
					this.Aggregator = new Aggregator( factory.NewIndicator( process ) );
			  }

			  public virtual ProgressListener ProgressForPart( string part, long totalCount )
			  {
					AssertNotBuilt();
					AssertUniquePart( part );
					ProgressListener_MultiPartProgressListener progress = new ProgressListener_MultiPartProgressListener( Aggregator, part, totalCount );
					Aggregator.add( progress, totalCount );
					return progress;
			  }

			  public virtual ProgressListener ProgressForUnknownPart( string part )
			  {
					AssertNotBuilt();
					AssertUniquePart( part );
					ProgressListener progress = ProgressListener_Fields.None;
					Aggregator.add( progress, 0 );
					return progress;
			  }

			  internal virtual void AssertUniquePart( string part )
			  {
					if ( !Parts.Add( part ) )
					{
						 throw new System.ArgumentException( string.Format( "Part '{0}' has already been defined.", part ) );
					}
			  }

			  internal virtual void AssertNotBuilt()
			  {
					if ( Aggregator == null )
					{
						 throw new System.InvalidOperationException( "Builder has been completed." );
					}
			  }

			  public virtual void Build()
			  {
					if ( Aggregator != null )
					{
						 Aggregator.initialize();
					}
					Aggregator = null;
					Parts = null;
			  }
		 }
	}

}