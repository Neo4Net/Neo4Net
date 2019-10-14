using System;
using System.Collections.Generic;

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
namespace Neo4Net.Graphalgo.impl.util
{

	using NoneStrictMath = Neo4Net.Kernel.impl.util.NoneStrictMath;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.Graphalgo.impl.util.PathInterest_PriorityBasedPathInterest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.Graphalgo.impl.util.PathInterest_VisitCountBasedPathInterest;

	/// <summary>
	/// @author Anton Persson
	/// </summary>
	public class PathInterestFactory
	{
		 public static readonly IComparer<IComparable> StandardComparator = IComparable.compareTo;

		 private PathInterestFactory()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static PathInterest<? extends Comparable> single()
		 public static PathInterest<IComparable> Single()
		 {
			  return SINGLE;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static PathInterest<? extends Comparable> allShortest()
		 public static PathInterest<IComparable> AllShortest()
		 {
			  return ALL_SHORTEST;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static PathInterest<? extends Comparable> all()
		 public static PathInterest<IComparable> All()
		 {
			  return ALL;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static final PathInterest<? extends Comparable> SINGLE = new PathInterest<Comparable>()
		 private static readonly PathInterest<IComparable> SINGLE = new PathInterestAnonymousInnerClass();

		 private class PathInterestAnonymousInnerClass : PathInterest<IComparable>
		 {
			 public IComparer<IComparable> comparator()
			 {
				  return StandardComparator;
			 }

			 public bool canBeRuledOut( int numberOfVisits, IComparable pathPriority, IComparable oldPriority )
			 {
				  return numberOfVisits > 0 || pathPriority.CompareTo( oldPriority ) >= 0;
			 }

			 public bool stillInteresting( int numberOfVisits )
			 {
				  return numberOfVisits <= 1;
			 }

			 public bool stopAfterLowestCost()
			 {
				  return true;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static final PathInterest<? extends Comparable> ALL_SHORTEST = new PriorityBasedPathInterest<Comparable>()
		 private static readonly PathInterest<IComparable> ALL_SHORTEST = new PriorityBasedPathInterestAnonymousInnerClass();

		 private class PriorityBasedPathInterestAnonymousInnerClass : PriorityBasedPathInterest<IComparable>
		 {
			 private System.Func<IComparable, IComparable, bool> interestFunction;

			 public override System.Func<IComparable, IComparable, bool> interestFunction()
			 {
				  if ( interestFunction == null )
				  {
						interestFunction = ( newValue, oldValue ) => newValue.compareTo( oldValue ) <= 0;
				  }
				  return interestFunction;
			 }

			 public override IComparer<IComparable> comparator()
			 {
				  return StandardComparator;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static final PathInterest<? extends Comparable> ALL = new PathInterest<Comparable>()
		 private static readonly PathInterest<IComparable> ALL = new PathInterestAnonymousInnerClass2();

		 private class PathInterestAnonymousInnerClass2 : PathInterest<IComparable>
		 {
			 public IComparer<IComparable> comparator()
			 {
				  return StandardComparator;
			 }

			 public bool canBeRuledOut( int numberOfVisits, IComparable pathPriority, IComparable oldPriority )
			 {
				  return false;
			 }

			 public bool stillInteresting( int numberOfVisits )
			 {
				  return true;
			 }

			 public bool stopAfterLowestCost()
			 {
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <P extends Comparable<? super P>> PathInterest<P> numberOfShortest(final int numberOfWantedPaths)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static PathInterest<P> NumberOfShortest<P>( int numberOfWantedPaths )
		 {
			  if ( numberOfWantedPaths < 1 )
			  {
					throw new System.ArgumentException( "Can not create PathInterest with interested in less than 1 path." );
			  }

			  return new VisitCountBasedPathInterestAnonymousInnerClass( numberOfWantedPaths );
		 }

		 private class VisitCountBasedPathInterestAnonymousInnerClass : VisitCountBasedPathInterest<P>
		 {
			 private int _numberOfWantedPaths;

			 public VisitCountBasedPathInterestAnonymousInnerClass( int numberOfWantedPaths )
			 {
				 this._numberOfWantedPaths = numberOfWantedPaths;
			 }

			 private IComparer<P> comparator = IComparable.compareTo;

			 internal override int numberOfWantedPaths()
			 {
				  return _numberOfWantedPaths;
			 }

			 public override IComparer<P> comparator()
			 {
				  return comparator;
			 }
		 }

		 public static PathInterest<double> AllShortest( double epsilon )
		 {
			  return new PriorityBasedTolerancePathInterest( epsilon );
		 }

		 public static PathInterest<double> All( double epsilon )
		 {
			  return new AllTolerancePathInterest( epsilon );
		 }

		 public static PathInterest<double> NumberOfShortest( double epsilon, int numberOfWantedPaths )
		 {
			  return new VisitCountBasedTolerancePathInterest( epsilon, numberOfWantedPaths );
		 }

		 public static PathInterest<double> Single( double epsilon )
		 {
			  return new SingleTolerancePathInterest( epsilon );
		 }

		 private class PriorityBasedTolerancePathInterest : PriorityBasedPathInterest<double>
		 {
			  internal readonly double Epsilon;
			  internal System.Func<double, double, bool> interestFunction = ( double? newValue, double? oldValue ) =>
			  {
							return NoneStrictMath.compare( newValue, oldValue, Epsilon ) <= 0;
			  };
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IComparer<double> ComparatorConflict;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: PriorityBasedTolerancePathInterest(final double epsilon)
			  internal PriorityBasedTolerancePathInterest( double epsilon )
			  {
					this.Epsilon = epsilon;
					this.ComparatorConflict = new NoneStrictMath.CommonToleranceComparator( epsilon );
			  }

			  public override System.Func<double, double, bool> InterestFunction()
			  {
					return interestFunction;
			  }

			  public override IComparer<double> Comparator()
			  {
					return ComparatorConflict;
			  }
		 }

		 private class VisitCountBasedTolerancePathInterest : VisitCountBasedPathInterest<double>
		 {
			  internal readonly double Epsilon;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int NumberOfWantedPathsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IComparer<double> ComparatorConflict;

			  internal VisitCountBasedTolerancePathInterest( double epsilon, int numberOfWantedPaths )
			  {
					this.Epsilon = epsilon;
					this.NumberOfWantedPathsConflict = numberOfWantedPaths;
					this.ComparatorConflict = new NoneStrictMath.CommonToleranceComparator( epsilon );
			  }

			  internal override int NumberOfWantedPaths()
			  {
					return NumberOfWantedPathsConflict;
			  }

			  public override IComparer<double> Comparator()
			  {
					return ComparatorConflict;
			  }
		 }

		 private class SingleTolerancePathInterest : PathInterest<double>
		 {
			  internal readonly double Epsilon;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IComparer<double> ComparatorConflict;

			  internal SingleTolerancePathInterest( double epsilon )
			  {
					this.Epsilon = epsilon;
					this.ComparatorConflict = new NoneStrictMath.CommonToleranceComparator( epsilon );
			  }

			  public override IComparer<double> Comparator()
			  {
					return ComparatorConflict;
			  }

			  public override bool CanBeRuledOut( int numberOfVisits, double? pathPriority, double? oldPriority )
			  {
					return numberOfVisits > 0 || NoneStrictMath.compare( pathPriority.Value, oldPriority.Value, Epsilon ) >= 0;
			  }

			  public override bool StillInteresting( int numberOfVisits )
			  {
					return numberOfVisits <= 1;
			  }

			  public override bool StopAfterLowestCost()
			  {
					return true;
			  }
		 }

		 private class AllTolerancePathInterest : PathInterest<double>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IComparer<double> ComparatorConflict;

			  internal AllTolerancePathInterest( double epsilon )
			  {
					this.ComparatorConflict = new NoneStrictMath.CommonToleranceComparator( epsilon );
			  }

			  public override IComparer<double> Comparator()
			  {
					return ComparatorConflict;
			  }

			  public override bool CanBeRuledOut( int numberOfVisits, double? pathPriority, double? oldPriority )
			  {
					return false;
			  }

			  public override bool StillInteresting( int numberOfVisits )
			  {
					return true;
			  }

			  public override bool StopAfterLowestCost()
			  {
					return false;
			  }
		 }
	}

}