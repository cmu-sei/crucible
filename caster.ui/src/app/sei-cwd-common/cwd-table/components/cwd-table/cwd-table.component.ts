/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {
  ChangeDetectionStrategy,
  Component,
  ContentChild,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  TemplateRef,
  ViewChild,
} from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import {
  fromMatPaginator,
  fromMatSort,
  paginateRows,
  sortRows,
} from '../../utils/datasource-utils';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { TableActionDirective } from '../../directives/table-action.directive';
import { TableItemActionDirective } from '../../directives/table-item-action.directive';
import { TableItemContentDirective } from '../../directives/table-item-content.directive';

@Component({
  selector: 'cwd-table',
  templateUrl: './cwd-table.component.html',
  styleUrls: ['./cwd-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CwdTableComponent<T> implements OnInit, OnChanges {
  public filterString: string;
  public datasource = new MatTableDataSource<T>(new Array<T>());

  // MatPaginator Output
  public defaultPageSize = 10;
  public pageEvent: PageEvent;
  displayedRows$: Observable<T[]>;
  totalRows$: Observable<number>;
  sortEvents$: Observable<Sort>;
  pageEvents$: Observable<PageEvent>;

  private isInitialized = false;

  @Input() items: T[];
  @Input() displayedColumns: (keyof T)[];
  @Input() loading: boolean;
  @Input() expandedItems: string[];
  @Input() getRowStyle: (item: T) => {};
  @Input() excludedAttributes: string[] = [];
  @Input() trackByPropertyName: string;
  @Output() expand: EventEmitter<{
    expand: boolean;
    item: T;
  }> = new EventEmitter<{ expand: boolean; item: T }>();
  @Output() add: EventEmitter<T> = new EventEmitter<T>();

  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;
  @ContentChild(TableActionDirective, { read: TemplateRef })
  tableActionTemplate;
  @ContentChild(TableItemActionDirective, { read: TemplateRef })
  tableActionItemTemplate;
  @ContentChild(TableItemContentDirective, { read: TemplateRef })
  tableItemContent;
  constructor() {}

  /**
   * Initialization
   */
  ngOnInit() {
    this.initialize();
  }

  private initialize() {
    if (this.isInitialized) {
      return;
    }

    this.datasource.data = this.items;

    this.datasource.filterPredicate = (data, filter: string) => {
      const accumulator = (currentTerm, key) => {
        let ret = '';
        if (!this.excludedAttributes.includes(key)) {
          ret += this.nestedFilterCheck(currentTerm, data, key);
        } else {
          ret += currentTerm;
        }
        return ret;
      };
      const dataStr = Object.keys(data).reduce(accumulator, '').toLowerCase();
      // Transform the filter by converting it to lowercase and removing whitespace.
      const transformedFilter = filter.trim().toLowerCase();
      return dataStr.indexOf(transformedFilter) !== -1;
    };
    this.sortEvents$ = fromMatSort(this.sort);
    this.pageEvents$ = fromMatPaginator(this.paginator);

    this.isInitialized = true;
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.initialize();

    if (changes.items) {
      this.datasource.data = changes.items.currentValue;

      this.filterAndSort();
    }
  }

  /**
   * Called by UI to add a filter to the DataSource
   * @param filterValue
   */
  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.datasource.filter = filterValue;
    this.filterAndSort();
  }

  nestedFilterCheck(search, data, key) {
    if (typeof data[key] === 'object') {
      for (const k in data[key]) {
        if (data[key][k] !== null) {
          search = this.nestedFilterCheck(search, data[key], k);
        }
      }
    } else {
      search += data[key];
    }
    return search;
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }

  /**
   * filters and sorts the displayed rows
   */
  filterAndSort() {
    if (this.datasource.filteredData) {
      const rows$ = of(this.datasource.filteredData);
      this.totalRows$ = rows$.pipe(map((rows) => rows.length));
      this.displayedRows$ = rows$.pipe(
        sortRows(this.sortEvents$),
        paginateRows(this.pageEvents$)
      );
    }
  }

  isExpanded(item) {
    return this.expandedItems ? this.expandedItems.includes(item.id) : false;
  }
  /**
   * function to emit the item when expanded or callapsed
   * @param event state of the panel
   * @param item data for the row.
   */
  expandedFn(event, item: T) {
    const expand = event;
    if (event && !this.isExpanded(item)) {
      this.expand.emit({ expand, item });
    }
    if (!expand && this.isExpanded(item)) {
      this.expand.emit({ expand, item });
    }
  }

  trackByFn(index, item) {
    if (this.trackByPropertyName != null) {
      return item[this.trackByPropertyName];
    } else {
      return index;
    }
  }

  afterExpand(event, item: T) {
    const expand = true;
    this.expand.emit({ expand, item });
  }

  afterCollapse(event, item: T) {
    const expand = false;
    this.expand.emit({ expand, item });
  }
}
