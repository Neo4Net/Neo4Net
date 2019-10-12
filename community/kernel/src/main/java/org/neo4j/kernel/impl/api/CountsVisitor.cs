/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.Impl.Api
{
	public interface CountsVisitor
	{

		 void VisitNodeCount( int labelId, long count );

		 void VisitRelationshipCount( int startLabelId, int typeId, int endLabelId, long count );

		 void VisitIndexStatistics( long indexId, long updates, long size );

		 void VisitIndexSample( long indexId, long unique, long size );
	}

	 public interface CountsVisitor_Visitable
	 {
		  void Accept( CountsVisitor visitor );
	 }

	 public class CountsVisitor_Adapter : CountsVisitor
	 {
		  public override void VisitNodeCount( int labelId, long count )
		  {
				// override in subclasses
		  }

		  public override void VisitRelationshipCount( int startLabelId, int typeId, int endLabelId, long count )
		  {
				// override in subclasses
		  }

		  public override void VisitIndexStatistics( long indexId, long updates, long size )
		  {
				// override in subclasses
		  }

		  public override void VisitIndexSample( long indexId, long unique, long size )
		  {
				// override in subclasses
		  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static CountsVisitor multiplex(final CountsVisitor... visitors)
		  public static CountsVisitor Multiplex( params CountsVisitor[] visitors )
		  {
				return new CountsVisitorAnonymousInnerClass( visitors );
		  }

		  private class CountsVisitorAnonymousInnerClass : CountsVisitor
		  {
			  private Org.Neo4j.Kernel.Impl.Api.CountsVisitor[] _visitors;

			  public CountsVisitorAnonymousInnerClass( Org.Neo4j.Kernel.Impl.Api.CountsVisitor[] visitors )
			  {
				  this._visitors = visitors;
			  }

			  public void visitNodeCount( int labelId, long count )
			  {
					foreach ( CountsVisitor visitor in _visitors )
					{
						 visitor.VisitNodeCount( labelId, count );
					}
			  }

			  public void visitRelationshipCount( int startLabelId, int typeId, int endLabelId, long count )
			  {
					foreach ( CountsVisitor visitor in _visitors )
					{
						 visitor.VisitRelationshipCount( startLabelId, typeId, endLabelId, count );
					}
			  }

			  public void visitIndexStatistics( long indexId, long updates, long size )
			  {
					foreach ( CountsVisitor visitor in _visitors )
					{
						 visitor.VisitIndexStatistics( indexId, updates, size );
					}
			  }

			  public void visitIndexSample( long indexId, long unique, long size )
			  {
					foreach ( CountsVisitor visitor in _visitors )
					{
						 visitor.VisitIndexSample( indexId, unique, size );
					}
			  }
		  }
	 }

}